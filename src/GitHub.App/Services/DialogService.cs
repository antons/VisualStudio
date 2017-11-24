using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
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

            if (connection != null)
            {
                await viewModel.InitializeAsync(connection);
                return (CloneDialogResult)await showDialog.Show(viewModel);
            }
            else
            {
                return (CloneDialogResult)await showDialog.ShowWithFirstConnection(viewModel);
            }
        }

        public async Task<string> ShowReCloneDialog(IRepositoryModel repository)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));

            var viewModel = serviceProvider.ExportProvider.GetExportedValue<IRepositoryRecloneViewModel>();
            viewModel.SelectedRepository = repository;
            return (string)await showDialog.ShowWithFirstConnection(viewModel);
        }

        public async Task ShowCreateGist()
        {
            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewGistCreationViewModel>();
            await showDialog.ShowWithFirstConnection(viewModel);
        }

        public async Task ShowCreateRepositoryDialog(IConnection connection)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));

            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewRepositoryCreationViewModel>();
            await viewModel.InitializeAsync(connection);
            await showDialog.Show(viewModel);
        }

        public async Task<IConnection> ShowLoginDialog()
        {
            var viewModel = serviceProvider.ExportProvider.GetExportedValue<INewLoginViewModel>();
            return (IConnection)await showDialog.Show(viewModel);
        }
    }
}
