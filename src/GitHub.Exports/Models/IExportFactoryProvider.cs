using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System;
using System.Windows;

namespace GitHub.Models
{
    public interface IExportFactoryProvider
    {
        IEnumerable<ExportFactory<IViewModel, IViewModelMetadata>> ViewModelFactory { get; set; }
        IEnumerable<ExportFactory<IView, IViewModelMetadata>> ViewFactory { get; set; }
        ExportLifetimeContext<IViewModel> GetViewModel(UIViewType viewType);
        ExportLifetimeContext<IView> GetView(UIViewType viewType);
        ExportLifetimeContext<FrameworkElement> CreateNewView(Type viewModelType);
    }
}
