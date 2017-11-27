using System;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;
using ReactiveUI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNewPullRequestCreationView : NewViewBase<INewPullRequestCreationViewModel, GenericNewPullRequestCreationView>
    { }

    [ExportViewFor(typeof(INewPullRequestCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewPullRequestCreationView : GenericNewPullRequestCreationView
    {
        public NewPullRequestCreationView()
        {
            InitializeComponent();
        }
    }
}
