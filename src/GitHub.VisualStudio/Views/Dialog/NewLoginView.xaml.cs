using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    /// <summary>
    /// Interaction logic for NewLoginView.xaml
    /// </summary>
    [ExportViewFor(typeof(INewLoginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewLoginView : UserControl
    {
        public NewLoginView()
        {
            InitializeComponent();
        }
    }
}
