using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

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
        readonly INavigationViewModel navigator;
        readonly SemaphoreSlim navigating = new SemaphoreSlim(1);
        readonly ObservableAsPropertyHelper<string> title;
        INewViewModel content;
        ILocalRepositoryModel localRepository;

        [ImportingConstructor]
        public NewGitHubPaneViewModel(
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiClientFactory,
            IConnectionManager connectionManager,
            ITeamExplorerServiceHolder teServiceHolder,
            INavigationViewModel navigator)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(teServiceHolder, nameof(teServiceHolder));
            Guard.ArgumentNotNull(navigator, nameof(navigator));

            this.serviceProvider = serviceProvider;
            this.apiClientFactory = apiClientFactory;
            this.connectionManager = connectionManager;
            this.teServiceHolder = teServiceHolder;
            this.navigator = navigator;

            // Gets navigator.Current if Content == navigator, otherwise null.
            var currentPage = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Content),
                navigator.WhenAnyValue(x => x.Content))
                .Select(x => x[0] == navigator ? x[1] as INewPanePageViewModel : null);

            title = currentPage
                .SelectMany(x => x?.WhenAnyValue(y => y.Title) ?? Observable.Return<string>(null))
                .Select(x => x ?? "GitHub")
                .ToProperty(this, x => x.Title);

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

        public async Task InitializeAsync()
        {
            await RepositoryChanged(teServiceHolder.ActiveRepo);
            teServiceHolder.Subscribe(this, x => RepositoryChanged(x).Forget());
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

        public async Task ShowPullRequests()
        {
            await navigating.WaitAsync();

            try
            {
                var viewModel = navigator.History.OfType<INewPullRequestListViewModel>().FirstOrDefault();

                if (viewModel == null)
                {
                    viewModel = serviceProvider.ExportProvider.GetExport<INewPullRequestListViewModel>().Value;
                    navigator.NavigateTo(viewModel);
                    await viewModel.InitializeAsync(LocalRepository, Connection);
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

        public async Task ShowPullRequest(string owner, string repo, int number)
        {
            await navigating.WaitAsync();

            try
            {
                var viewModel = navigator.History.OfType<INewPullRequestDetailViewModel>()
                    .Where(x => x.RemoteRepositoryOwner == owner && x.LocalRepository.Name == repo && x.Number == number)
                    .FirstOrDefault();

                if (viewModel == null)
                {
                    viewModel = serviceProvider.ExportProvider.GetExport<INewPullRequestDetailViewModel>().Value;
                    navigator.NavigateTo(viewModel);
                    await viewModel.InitializeAsync(LocalRepository, Connection, owner, repo, number);
                }
                else
                {
                    navigator.NavigateTo(viewModel);
                }

                navigator.NavigateTo(viewModel);
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
