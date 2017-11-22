using System;
using GitHub.Primitives;
using GitHub.ViewModels;

namespace GitHub.Services
{
    /// <summary>
    /// Service for displaying the GitHub for Visual Studio dialog.
    /// </summary>
    /// <remarks>
    /// This is a low-level service used by <see cref="IDialogService"/> to carry out the actual
    /// showing of the dialog. You probably want to use <see cref="IDialogService"/> instead if
    /// you want to show the dialog for login/clone etc.
    /// </remarks>
    public interface IShowDialogService
    {
        /// <summary>
        /// Shows a view model in the dialog.
        /// </summary>
        /// <param name="viewModel">The view model to show.</param>
        void Show(INewViewModel viewModel);

        /// <summary>
        /// Shows a view model that requires a connection in the dialog.
        /// </summary>
        /// <param name="viewModel">The view model to show.</param>
        /// <param name="hostAddress">
        /// The address of the host whose connection is required. If there is no existing
        /// connection to this address, the login dialog will be shown first.
        /// </param>
        void Show(IConnectionInitializedViewModel viewModel, HostAddress hostAddress);
    }
}
