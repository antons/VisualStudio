using System;
using System.Reactive;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents a view that can be shown in the GitHub for Visual Studio dialog.
    /// </summary>
    public interface IDialogContentViewModel : INewViewModel
    {
        /// <summary>
        /// Gets a title to display in the dialog titlebar.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets an observable that is signalled with a single value when the dialog should be
        /// closed.
        /// </summary>
        IObservable<Unit> Closed { get; }
    }
}
