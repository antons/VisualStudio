using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GitHub.ViewModels.GitHubPane
{
    public interface INewPullRequestListViewModel : INewSearchablePanePageViewModel, IOpenInBrowser
    {
        IReadOnlyList<IRemoteRepositoryModel> Repositories { get; }
        IRemoteRepositoryModel SelectedRepository { get; set; }
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        IReadOnlyList<PullRequestState> States { get; set; }
        PullRequestState SelectedState { get; set; }
        ObservableCollection<IAccount> Authors { get; }
        IAccount SelectedAuthor { get; set; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; set; }
        ReactiveCommand<object> OpenPullRequest { get; }
        ReactiveCommand<object> CreatePullRequest { get; }
        ReactiveCommand<object> OpenPullRequestOnGitHub { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
