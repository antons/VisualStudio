using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model that represents a page in the GitHub pane.
    /// </summary>
    public interface INewPanePageViewModel : INewViewModel
    {
        /// <summary>
        /// Gets a value indicating whether the page is busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets the title to display in the pane when the page is shown.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Refreshes the view model when first loaded.
        /// </summary>
        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);

        /// <summary>
        /// Refreshes the view model.
        /// </summary>
        Task Refresh();
    }
}