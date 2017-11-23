using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;

namespace GitHub.Services
{
    [Export(typeof(IDialogService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DialogService : IDialogService
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly IConnectionManager connectionManager;
        readonly IUIProvider uiProvider;
        readonly IShowDialogService showDialog;

        [ImportingConstructor]
        public DialogService(
            IGitHubServiceProvider serviceProvider,
            IConnectionManager connectionManager,
            IUIProvider uiProvider,
            IShowDialogService showDialog)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(uiProvider, nameof(uiProvider));
            Guard.ArgumentNotNull(showDialog, nameof(showDialog));

            this.serviceProvider = serviceProvider;
            this.connectionManager = connectionManager;
            this.uiProvider = uiProvider;
            this.showDialog = showDialog;
        }

        public async Task<CloneDialogResult> ShowCloneDialog(IConnection connection)
        {
            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewRepositoryCloneViewModel>();
            return (CloneDialogResult)await showDialog.ShowWithConnection(viewModel);
        }

        public Task<string> ShowReCloneDialog(IRepositoryModel repository)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));

            var controller = uiProvider.Configure(UIControllerFlow.ReClone);
            var basePath = default(string);

            controller.TransitionSignal.Subscribe(x =>
            {
                var vm = x.View.ViewModel as IBaseCloneViewModel;

                if (vm != null)
                {
                    vm.SelectedRepository = repository;
                }

                vm.Done.Subscribe(_ =>
                {
                    basePath = vm?.BaseRepositoryPath;
                });
            });

            uiProvider.RunInDialog(controller);

            return Task.FromResult(basePath);
        }

        public async Task<IConnection> ShowLoginDialog()
        {
            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewLoginViewModel>();
            return (IConnection)await showDialog.Show(viewModel);
        }

        public async Task ShowCreateGist()
        {
            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewGistCreationViewModel>();
            await showDialog.ShowWithConnection(viewModel);
        }
    }
}
