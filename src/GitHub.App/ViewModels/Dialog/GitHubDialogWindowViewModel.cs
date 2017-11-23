using System;
using System.Reactive;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    public class GitHubDialogWindowViewModel : NewViewModelBase
    {
        IDialogContentViewModel content;

        public GitHubDialogWindowViewModel()
        {
            Closed = this.WhenAnyObservable(x => x.Content.Closed);
        }

        public IDialogContentViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        public IObservable<Unit> Closed { get; }

        public void Initialize(IDialogContentViewModel viewModel)
        {
            Content = viewModel;
        }

        public void Initialize(IConnectionInitializedViewModel viewModel, HostAddress hostAddress)
        {
        }
    }
}
