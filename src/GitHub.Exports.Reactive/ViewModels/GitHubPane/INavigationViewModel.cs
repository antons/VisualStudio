using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that supports back/forward navigation of an inner content page.
    /// </summary>
    public interface INavigationViewModel : INewViewModel
    {
        /// <summary>
        /// Gets or sets the current content page.
        /// </summary>
        INewPanePageViewModel Content { get; }

        /// <summary>
        /// Gets or sets the current index in the history list.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets the back and forward history.
        /// </summary>
        IReactiveList<INewPanePageViewModel> History { get; }

        /// <summary>
        /// Gets a command that navigates back in the history.
        /// </summary>
        ReactiveCommand<object> NavigateBack { get; }

        /// <summary>
        /// Gets a command that navigates forwards in the history.
        /// </summary>
        ReactiveCommand<object> NavigateForward { get; }

        /// <summary>
        /// Navigates back if possible.
        /// </summary>
        /// <returns>True if there was a page to navigate back to.</returns>
        bool Back();

        /// <summary>
        /// Clears the current page and all history .
        /// </summary>
        void Clear();

        /// <summary>
        /// Navigates forwards if possible.
        /// </summary>
        /// <returns>True if there was a page to navigate forwards to.</returns>
        bool Forward();

        /// <summary>
        /// Navigates to a new page.
        /// </summary>
        /// <param name="page">The page view model.</param>
        void NavigateTo(INewPanePageViewModel page);
    }
}