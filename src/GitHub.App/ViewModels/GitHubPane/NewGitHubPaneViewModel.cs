using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using ReactiveUI;
using OleMenuCommand = Microsoft.VisualStudio.Shell.OleMenuCommand;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(INewGitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NewGitHubPaneViewModel : NewViewModelBase, INewGitHubPaneViewModel
    {
        static readonly Regex pullUri = CreateRoute("/:owner/:repo/pull/:number");

        readonly IGitHubServiceProvider serviceProvider;
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IConnectionManager connectionManager;
        readonly ITeamExplorerServiceHolder teServiceHolder;
        readonly IVisualStudioBrowser browser;
        readonly IUsageTracker usageTracker;
        readonly INavigationViewModel navigator;
        readonly SemaphoreSlim navigating = new SemaphoreSlim(1);
        readonly ObservableAsPropertyHelper<string> title;
        readonly ReactiveCommand<Unit> refresh;
        readonly ReactiveCommand<Unit> showPullRequests;
        readonly ReactiveCommand<object> openInBrowser;
        INewViewModel content;
        ILocalRepositoryModel localRepository;

        [ImportingConstructor]
        public NewGitHubPaneViewModel(
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiClientFactory,
            IConnectionManager connectionManager,
            ITeamExplorerServiceHolder teServiceHolder,
            IVisualStudioBrowser browser,
            IUsageTracker usageTracker,
            INavigationViewModel navigator)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(teServiceHolder, nameof(teServiceHolder));
            Guard.ArgumentNotNull(browser, nameof(browser));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(navigator, nameof(navigator));

            this.serviceProvider = serviceProvider;
            this.apiClientFactory = apiClientFactory;
            this.connectionManager = connectionManager;
            this.teServiceHolder = teServiceHolder;
            this.browser = browser;
            this.usageTracker = usageTracker;
            this.navigator = navigator;

            // Returns navigator.Current if Content == navigator, otherwise null.
            var currentPage = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Content),
                navigator.WhenAnyValue(x => x.Content))
                .Select(x => x[0] == navigator ? x[1] as INewPanePageViewModel : null);

            title = currentPage
                .SelectMany(x => x?.WhenAnyValue(y => y.Title) ?? Observable.Return<string>(null))
                .Select(x => x ?? "GitHub")
                .ToProperty(this, x => x.Title);

            refresh = ReactiveCommand.CreateAsyncTask(
                currentPage.SelectMany(x => x?.WhenAnyValue(
                        y => y.IsLoading,
                        y => y.IsBusy,
                        (loading, busy) => !loading && !busy)
                            ?? Observable.Return(false)),
                _ => navigator.Content.Refresh());

            showPullRequests = ReactiveCommand.CreateAsyncTask(
                this.WhenAny(x => x.Content, x => x.Value == navigator),
                _ => ShowPullRequests());

            openInBrowser = ReactiveCommand.Create(currentPage.Select(x => x is IOpenInBrowser));
            openInBrowser.Subscribe(_ => browser.OpenUrl(((IOpenInBrowser)navigator.Content).WebUrl));

            navigator.WhenAnyObservable(x => x.Content.NavigationRequested)
                .Subscribe(x => NavigateTo(x).Forget());
        }

        public IConnection Connection
        {
            get;
            private set;
        }

        public INewViewModel Content
        {
            get { return content; }
            protected set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        public ILocalRepositoryModel LocalRepository
        {
            get { return localRepository; }
            private set { this.RaiseAndSetIfChanged(ref localRepository, value); }
        }

        public string Title => title.Value;

        public async Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            await RepositoryChanged(teServiceHolder.ActiveRepo);
            teServiceHolder.Subscribe(this, x => RepositoryChanged(x).Forget());
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.pullRequestCommand, showPullRequests);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.backCommand, navigator.NavigateBack);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.forwardCommand, navigator.NavigateForward);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.refreshCommand, refresh);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.githubCommand, openInBrowser);

            serviceProvider.AddCommandHandler(Guids.guidGitHubToolbarCmdSet, PkgCmdIDList.helpCommand,
                 (_, __) =>
                 {
                     browser.OpenUrl(new Uri(GitHubUrls.Documentation));
                     usageTracker.IncrementCounter(x => x.NumberOfGitHubPaneHelpClicks).Forget();
                 });
        }

        public async Task NavigateTo(Uri uri)
        {
            Guard.ArgumentNotNull(uri, nameof(uri));

            if (uri.Scheme != "github")
            {
                throw new NotSupportedException("Invalid URI scheme for GitHub pane: " + uri.Scheme);
            }

            if (uri.Authority != "pane")
            {
                throw new NotSupportedException("Invalid URI authority for GitHub pane: " + uri.Authority);
            }

            Match match;

            if (uri.AbsolutePath == "/pulls")
            {
                await ShowPullRequests();
            }
            else if ((match = pullUri.Match(uri.AbsolutePath))?.Success == true)
            {
                var owner = match.Groups["owner"].Value;
                var repo = match.Groups["repo"].Value;
                var number = int.Parse(match.Groups["number"].Value);
                await ShowPullRequest(owner, repo, number);
            }
        }

        public Task ShowPullRequests()
        {
            return NavigateTo<INewPullRequestListViewModel>(x => x.InitializeAsync(LocalRepository, Connection)); 
        }

        public Task ShowPullRequest(string owner, string repo, int number)
        {
            return NavigateTo<INewPullRequestDetailViewModel>(
                x => x.InitializeAsync(LocalRepository, Connection, owner, repo, number),
                x => x.RemoteRepositoryOwner == owner && x.LocalRepository.Name == repo && x.Number == number);
        }

        OleMenuCommand BindNavigatorCommand<T>(IServiceProvider serviceProvider, int commandId, ReactiveCommand<T> command)
        {
            Func<bool> canExecute = () => Content == navigator && command.CanExecute(null);

            var result = serviceProvider.AddCommandHandler(
                Guids.guidGitHubToolbarCmdSet,
                commandId,
                canExecute,
                () => command.Execute(null),
                true);

            Observable.CombineLatest(
                this.WhenAnyValue(x => x.Content),
                command.CanExecuteObservable,
                (c, e) => c == navigator && e)
                .Subscribe(x => result.Enabled = x);

            return result;
        }

        async Task NavigateTo<TViewModel>(Func<TViewModel, Task> initialize, Func<TViewModel, bool> match = null)
            where TViewModel : INewPanePageViewModel
        {
            await navigating.WaitAsync();

            try
            {
                var viewModel = navigator.History
                    .OfType<TViewModel>()
                    .FirstOrDefault(x => match?.Invoke(x) ?? true);

                if (viewModel == null)
                {
                    viewModel = serviceProvider.ExportProvider.GetExport<TViewModel>().Value;
                    navigator.NavigateTo(viewModel);
                    await initialize(viewModel);
                }
                else
                {
                    navigator.NavigateTo(viewModel);
                }
            }
            finally
            {
                navigating.Release();
            }
        }

        async Task RepositoryChanged(ILocalRepositoryModel repository)
        {
            LocalRepository = repository;
            Connection = null;

            if (repository == null)
            {
                //Content = NotAGitRepositoryViewModel.Instance;
                return;
            }

            var repositoryUrl = repository.CloneUrl.ToRepositoryUrl();
            var isDotCom = HostAddress.IsGitHubDotComUri(repositoryUrl);
            var client = await apiClientFactory.Create(repository.CloneUrl);
            var isEnterprise = isDotCom ? false : client.IsEnterprise();

            if (isDotCom || isEnterprise)
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);

                Connection = await connectionManager.GetConnection(hostAddress);

                // TODO: Handle repository not found.
                if (Connection != null)
                {
                    navigator.Clear();
                    Content = navigator;
                    await ShowPullRequests();
                }
                else
                {
                    //Content = new LoggedOutViewModel(hostAddress);
                }
            }
            else
            {
                //Content = NotAGitHubRepositoryViewModel.Instance;
            }
        }

        static Regex CreateRoute(string route)
        {
            // Build RegEx from route (:foo to named group (?<foo>[a-z0-9]+)).
            var routeFormat = new Regex("(:([a-z]+))\\b").Replace(route, "(?<$2>[a-z0-9]+)");
            return new Regex(routeFormat, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }
    }
}
