using System;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    public class GitHubDialogWindowViewModel : NewViewModelBase
    {
        INewViewModel content;

        public INewViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        public void Initialize(INewViewModel viewModel)
        {
            Content = viewModel;
        }

        public void Initialize(IConnectionInitializedViewModel viewModel, HostAddress hostAddress)
        {
        }
    }
}
