using System;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Base class for view models that appear as a page in a the GitHub pane.
    /// </summary>
    public abstract class NewPanePageViewModelBase : NewViewModelBase, INewPanePageViewModel
    {
        bool isBusy;
        string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanePageViewModelBase"/> class.
        /// </summary>
        protected NewPanePageViewModelBase()
        {
        }

        /// <inheritdoc/>
        public bool IsBusy
        {
            get { return isBusy; }
            protected set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public abstract Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);

        /// <inheritdoc/>
        public abstract Task Refresh();
    }
}
