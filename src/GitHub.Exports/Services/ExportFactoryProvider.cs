using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.Models;
using System.Windows;
using System;

namespace GitHub.Services
{
    [Export(typeof(IExportFactoryProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExportFactoryProvider : IExportFactoryProvider
    {
        [ImportingConstructor]
        public ExportFactoryProvider(ICompositionService cc)
        {
            cc.SatisfyImportsOnce(this);
        }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IViewModel, IViewModelMetadata>> ViewModelFactory { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IView, IViewModelMetadata>> ViewFactory { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<FrameworkElement, INewViewModelMetadata>> NewViewFactory { get; set; }

        public ExportLifetimeContext<IViewModel> GetViewModel(UIViewType viewType)
        {
            if (ViewModelFactory == null)
            {
                throw new GitHubLogicException("Attempted to obtain a view model before we imported the ViewModelFactory");
            }

            var f = ViewModelFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);

            if (f == null)
            {
                throw new GitHubLogicException(string.Format(CultureInfo.InvariantCulture, "Could not locate view model for {0}.", viewType));
            }

            return f.CreateExport();
        }

        public ExportLifetimeContext<IView> GetView(UIViewType viewType)
        {
            var f = ViewFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);

            if (f == null)
            {
                throw new GitHubLogicException(string.Format(CultureInfo.InvariantCulture, "Could not locate view for {0}.", viewType));
            }

            return f.CreateExport();
        }

        public ExportLifetimeContext<FrameworkElement> CreateNewView(Type viewModelType)
        {
            var f = NewViewFactory.FirstOrDefault(x => x.Metadata.ViewModelType == viewModelType);
            return f?.CreateExport();
        }
    }
}
