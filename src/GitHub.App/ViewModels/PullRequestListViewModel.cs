﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.App;
using GitHub.Collections;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.UI;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : PanePageViewModelBase, IPullRequestListViewModel, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestListViewModel>();

        readonly IConnection connection;
        readonly IModelServiceFactory modelServiceFactory;
        readonly ILocalRepositoryModel localRepository;
        readonly TrackingCollection<IAccount> trackingAuthors;
        readonly TrackingCollection<IAccount> trackingAssignees;
        readonly IPackageSettings settings;
        readonly IVisualStudioBrowser visualStudioBrowser;
        readonly PullRequestListUIState listSettings;
        readonly bool constructing;
        IRemoteRepositoryModel remoteRepository;
        IModelService modelService;

        [ImportingConstructor]
        PullRequestListViewModel(
            IGlobalConnection connection,
            IModelServiceFactory modelServiceFactory,
            ITeamExplorerServiceHolder teservice,
            IPackageSettings settings,
            IVisualStudioBrowser visualStudioBrowser)
            : this(connection.Get(), modelServiceFactory, teservice.ActiveRepo, settings, visualStudioBrowser)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(teservice, nameof(teservice));
            Guard.ArgumentNotNull(settings, nameof(settings));
        }

        public PullRequestListViewModel(
            IConnection connection,
            IModelServiceFactory modelServiceFactory,
            ILocalRepositoryModel repository,
            IPackageSettings settings,
            IVisualStudioBrowser visualStudioBrowser)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(settings, nameof(settings));
            Guard.ArgumentNotNull(visualStudioBrowser, nameof(visualStudioBrowser));

            constructing = true;
            this.connection = connection;
            this.modelServiceFactory = modelServiceFactory;
            this.localRepository = repository;
            this.settings = settings;
            this.visualStudioBrowser = visualStudioBrowser;

            Title = Resources.PullRequestsNavigationItemText;

            this.listSettings = settings.UIState
                .GetOrCreateRepositoryState(repository.CloneUrl)
                .PullRequests;

            States = new List<PullRequestState> {
                new PullRequestState { IsOpen = true, Name = "Open" },
                new PullRequestState { IsOpen = false, Name = "Closed" },
                new PullRequestState { Name = "All" }
            };

            trackingAuthors = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(),
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAssignees = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(),
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAuthors.Subscribe();
            trackingAssignees.Subscribe();

            Authors = trackingAuthors.CreateListenerCollection(EmptyUser, this.WhenAnyValue(x => x.SelectedAuthor));
            Assignees = trackingAssignees.CreateListenerCollection(EmptyUser, this.WhenAnyValue(x => x.SelectedAssignee));

            CreatePullRequests();

            this.WhenAny(x => x.SelectedState, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor, SearchQuery));

            this.WhenAny(x => x.SelectedAssignee, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor, SearchQuery));

            this.WhenAny(x => x.SelectedAuthor, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a, SearchQuery));

            this.WhenAny(x => x.SearchQuery, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(f => UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, f));

            SelectedState = States.FirstOrDefault(x => x.Name == listSettings.SelectedState) ?? States[0];
            OpenPullRequest = ReactiveCommand.Create();
            OpenPullRequest.Subscribe(DoOpenPullRequest);
            CreatePullRequest = ReactiveCommand.Create();
            CreatePullRequest.Subscribe(_ => DoCreatePullRequest());

            OpenPullRequestOnGitHub = ReactiveCommand.Create();
            OpenPullRequestOnGitHub.Subscribe(x => DoOpenPullRequestOnGitHub((int)x));

            constructing = false;
        }

        public override void Initialize(ViewWithData data)
        {
            base.Initialize(data);
            Load().Forget();
        }

        async Task Load()
        {
            IsBusy = true;

            if (modelService == null)
            {
                modelService = await modelServiceFactory.CreateAsync(connection);
            }

            if (remoteRepository == null)
            {
                remoteRepository = await modelService.GetRepository(
                    localRepository.Owner,
                    localRepository.Name);
                Repositories = remoteRepository.IsFork ?
                    new[] { remoteRepository.Parent, remoteRepository } :
                    new[] { remoteRepository };
                SelectedRepository = Repositories[0];
            }

            PullRequests = modelService.GetPullRequests(SelectedRepository, pullRequests);
            pullRequests.Subscribe(pr =>
            {
                trackingAssignees.AddItem(pr.Assignee);
                trackingAuthors.AddItem(pr.Author);
            }, () => { });

            pullRequests.OriginalCompleted
                .ObserveOn(RxApp.MainThreadScheduler)
                ////.Catch<System.Reactive.Unit, Octokit.AuthorizationException>(ex =>
                ////{
                ////    log.Info("Received AuthorizationException reading pull requests", ex);
                ////    return repositoryHost.LogOut();
                ////})
                .Catch<System.Reactive.Unit, Exception>(ex =>
                {
                    // Occurs on network error, when the repository was deleted on GitHub etc.
                    log.Error(ex, "Received Exception reading pull requests");
                    return Observable.Empty<System.Reactive.Unit>();
                })
                .Subscribe(_ =>
                {
                    if (listSettings.SelectedAuthor != null)
                    {
                        SelectedAuthor = Authors.FirstOrDefault(x => x.Login == listSettings.SelectedAuthor);
                    }

                    if (listSettings.SelectedAssignee != null)
                    {
                        SelectedAssignee = Assignees.FirstOrDefault(x => x.Login == listSettings.SelectedAssignee);
                    }

                    IsBusy = false;
                    UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, SearchQuery);
                });
        }

        void UpdateFilter(PullRequestState state, IAccount ass, IAccount aut, string filText)
        {
            if (PullRequests == null)
                return;

            var filterTextIsNumber = false;
            var filterTextIsString = false;
            var filterPullRequestNumber = 0;

            if (filText != null)
            {
                filText = filText.Trim();

                var hasText = !string.IsNullOrEmpty(filText);

                if (hasText && filText.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
                {
                    filterTextIsNumber = int.TryParse(filText.Substring(1), out filterPullRequestNumber);
                }
                else
                {
                    filterTextIsNumber = int.TryParse(filText, out filterPullRequestNumber);
                }

                filterTextIsString = hasText && !filterTextIsNumber;
            }

            pullRequests.Filter = (pullRequest, index, list) =>
                (!state.IsOpen.HasValue || state.IsOpen == pullRequest.IsOpen) &&
                (ass == null || ass.Equals(pullRequest.Assignee)) &&
                (aut == null || aut.Equals(pullRequest.Author)) &&
                (filterTextIsNumber == false || pullRequest.Number == filterPullRequestNumber) && 
                (filterTextIsString == false || pullRequest.Title.ToUpperInvariant().Contains(filText.ToUpperInvariant()));
        }

        string searchQuery;
        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            private set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        IReadOnlyList<IRemoteRepositoryModel> repositories;
        public IReadOnlyList<IRemoteRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, value); }
        }

        IRemoteRepositoryModel selectedRepository;
        public IRemoteRepositoryModel SelectedRepository
        {
            get { return selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref selectedRepository, value); }
        }

        ITrackingCollection<IPullRequestModel> pullRequests;
        public ITrackingCollection<IPullRequestModel> PullRequests
        {
            get { return pullRequests; }
            private set { this.RaiseAndSetIfChanged(ref pullRequests, value); }
        }

        IPullRequestModel selectedPullRequest;
        public IPullRequestModel SelectedPullRequest
        {
            get { return selectedPullRequest; }
            set { this.RaiseAndSetIfChanged(ref selectedPullRequest, value); }
        }

        IReadOnlyList<PullRequestState> states;
        public IReadOnlyList<PullRequestState> States
        {
            get { return states; }
            set { this.RaiseAndSetIfChanged(ref states, value); }
        }

        PullRequestState selectedState;
        public PullRequestState SelectedState
        {
            get { return selectedState; }
            set { this.RaiseAndSetIfChanged(ref selectedState, value); }
        }

        ObservableCollection<IAccount> assignees;
        public ObservableCollection<IAccount> Assignees
        {
            get { return assignees; }
            set { this.RaiseAndSetIfChanged(ref assignees, value); }
        }

        ObservableCollection<IAccount> authors;
        public ObservableCollection<IAccount> Authors
        {
            get { return authors; }
            set { this.RaiseAndSetIfChanged(ref authors, value); }
        }

        IAccount selectedAuthor;
        public IAccount SelectedAuthor
        {
            get { return selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref selectedAuthor, value); }
        }

        IAccount selectedAssignee;
        public IAccount SelectedAssignee
        {
            get { return selectedAssignee; }
            set { this.RaiseAndSetIfChanged(ref selectedAssignee, value); }
        }

        IAccount emptyUser = new Account("[None]", false, false, 0, 0, Observable.Empty<BitmapSource>());
        public IAccount EmptyUser
        {
            get { return emptyUser; }
        }

        public bool IsSearchEnabled => true;

        readonly Subject<ViewWithData> navigate = new Subject<ViewWithData>();
        public IObservable<ViewWithData> Navigate => navigate;

        public ReactiveCommand<object> OpenPullRequest { get; }
        public ReactiveCommand<object> CreatePullRequest { get; }

        public ReactiveCommand<object> OpenPullRequestOnGitHub { get; }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                pullRequests.Dispose();
                trackingAuthors.Dispose();
                trackingAssignees.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void CreatePullRequests()
        {
            PullRequests = new TrackingCollection<IPullRequestModel>();
            pullRequests.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            pullRequests.NewerComparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
        }

        void ResetAndLoad()
        {
            CreatePullRequests();
            UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, SearchQuery);
            Load().Forget();
        }

        void SaveSettings()
        {
            if (!constructing)
            {
                listSettings.SelectedState = SelectedState.Name;
                listSettings.SelectedAssignee = SelectedAssignee?.Login;
                listSettings.SelectedAuthor = SelectedAuthor?.Login;
                settings.Save();
            }
        }

        void DoOpenPullRequest(object pullRequest)
        {
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));

            var d = new ViewWithData(UIControllerFlow.PullRequestDetail)
            {
                Data = new PullRequestDetailArgument
                {
                    RepositoryOwner = SelectedRepository.Owner,
                    Number = (int)pullRequest,
                }
            };

            navigate.OnNext(d);
        }

        void DoCreatePullRequest()
        {
            var d = new ViewWithData(UIControllerFlow.PullRequestCreation);
            navigate.OnNext(d);
        }

        void DoOpenPullRequestOnGitHub(int pullRequest)
        {
            var repoUrl = SelectedRepository.CloneUrl.ToRepositoryUrl();
            var url = repoUrl.Append("pull/" + pullRequest);
            visualStudioBrowser.OpenUrl(url);
        }
    }
}
