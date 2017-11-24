using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
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
        readonly IGitHubServiceProvider serviceProvider;
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IConnectionManager connectionManager;
        readonly ITeamExplorerServiceHolder teServiceHolder;
        INewViewModel content;

        [ImportingConstructor]
        public NewGitHubPaneViewModel(
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiClientFactory,
            IConnectionManager connectionManager,
            ITeamExplorerServiceHolder teServiceHolder)
        {
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(teServiceHolder, nameof(teServiceHolder));

            this.serviceProvider = serviceProvider;
            this.apiClientFactory = apiClientFactory;
            this.connectionManager = connectionManager;
            this.teServiceHolder = teServiceHolder;
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
            get;
            private set;
        }

        public async Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            await RepositoryChanged(teServiceHolder.ActiveRepo);
            teServiceHolder.Subscribe(this, x => RepositoryChanged(x).Forget());
        }

        public async Task ShowPullRequests()
        {
            var viewModel = serviceProvider.ExportProvider.GetExport<INewPullRequestListViewModel>().Value;
            //Content = SpinnerViewModel.Instance;
            await viewModel.InitializeAsync(LocalRepository, Connection);
            Content = viewModel;
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
    }
}
