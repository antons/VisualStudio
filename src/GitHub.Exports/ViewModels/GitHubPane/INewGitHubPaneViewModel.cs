using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface INewGitHubPaneViewModel
    {
        IConnection Connection { get; }
        INewViewModel Content { get; }
        ILocalRepositoryModel LocalRepository { get; }

        Task InitializeAsync(IServiceProvider paneServiceProvider);
        Task ShowPullRequests();
    }
}