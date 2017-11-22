using System;
using System.ComponentModel.Composition;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using GitHub.VisualStudio.Views.Dialog;

namespace GitHub.VisualStudio.UI.Services
{
    [Export(typeof(IShowDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShowDialogService : IShowDialogService
    {
        public void Show(INewViewModel viewModel)
        {
            var dialogViewModel = new GitHubDialogWindowViewModel();
            dialogViewModel.Initialize(viewModel);

            var window = new GitHubDialogWindow(dialogViewModel);
            window.ShowModal();
        }

        public void Show(IConnectionInitializedViewModel viewModel, HostAddress hostAddress)
        {
            var dialogViewModel = new GitHubDialogWindowViewModel();
            dialogViewModel.Initialize(viewModel, hostAddress);

            var window = new GitHubDialogWindow(dialogViewModel);
            window.ShowModal();
        }
    }
}
