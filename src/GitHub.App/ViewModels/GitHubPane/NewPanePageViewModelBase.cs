using System;
using System.Reactive.Subjects;
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
        static readonly Uri paneUri = new Uri("github://pane");
        Subject <Uri> navigate = new Subject<Uri>();
        bool isBusy;
        bool isLoading;
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
        public bool IsLoading
        {
            get { return isLoading; }
            protected set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public IObservable<Uri> NavigationRequested => navigate;

        /// <inheritdoc/>
        public abstract Task Refresh();

        /// <summary>
        /// Sends a requests to navigate to a new page.
        /// </summary>
        /// <param name="uri">
        /// The path portion of the URI of the new page, e.g. "pulls".
        /// </param>
        protected void NavigateTo(string uri) => navigate.OnNext(new Uri(paneUri, uri));
    }
}
