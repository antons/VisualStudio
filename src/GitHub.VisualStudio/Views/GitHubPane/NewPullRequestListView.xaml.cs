using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNewPullRequestListView : NewViewBase<INewPullRequestListViewModel, NewPullRequestListView>
    { }

    [ExportViewFor(typeof(INewPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewPullRequestListView : GenericNewPullRequestListView, IDisposable
    {
        readonly Subject<int> open = new Subject<int>();
        readonly Subject<object> create = new Subject<object>();

        [ImportingConstructor]
        public NewPullRequestListView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
            });
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposed)
            {
                if (disposing)
                {
                    open.Dispose();
                    create.Dispose();
                }

                disposed = true;
            }
        }
    }
}
