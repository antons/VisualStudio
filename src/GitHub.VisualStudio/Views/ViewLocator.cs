using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GitHub.Exports;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views
{
    /// <summary>
    /// Locates a view for a view model.
    /// </summary>
    /// <remarks>
    /// A converter of this type should be placed in the resources for top-level GHfVS controls and
    /// is used as a default DataTemplate for finding a view for a view model. This is a variation
    /// on the MVVM Convention over Configuration pattern[1], here using MEF to locate the view.
    /// [1] http://stackoverflow.com/questions/768304
    /// </remarks>
    public class ViewLocator : IValueConverter
    {
        private static IExportFactoryProvider factoryProvider;

        /// <summary>
        /// Converts a view model into a view.
        /// </summary>
        /// <param name="value">The view model.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>
        /// A new instance of a view for the specified view model, or an error string if a view
        /// could not be located.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var exportViewModelAttribute = value.GetType().GetCustomAttributes(typeof(ExportAttribute), false)
                .OfType<ExportAttribute>()
                .Where(x => typeof(INewViewModel).IsAssignableFrom(x.ContractType))
                .FirstOrDefault();

            if (exportViewModelAttribute != null)
            {
                var factory = FactoryProvider?.CreateNewView(exportViewModelAttribute.ContractType);

                if (factory != null)
                {
                    var result = factory.Value;
                    result.DataContext = value;
                    return result;
                }
            }

            return $"Could not locate view for '{value.GetType()}'";
        }

        /// <summary>
        /// Not implemented in this class.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static IExportFactoryProvider FactoryProvider
        {
            get
            {
                if (factoryProvider == null)
                {
                    factoryProvider = GitHub.VisualStudio.Services.GitHubServiceProvider.TryGetService<IExportFactoryProvider>();
                }

                return factoryProvider;
            }
        }
    }
}
