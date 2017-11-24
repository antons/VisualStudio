using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(INewGitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewGitHubPaneView : UserControl
    {
        public NewGitHubPaneView()
        {
            InitializeComponent();
        }
    }
}
