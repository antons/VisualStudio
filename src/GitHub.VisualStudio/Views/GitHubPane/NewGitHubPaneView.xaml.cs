using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNewGitHubPaneView : NewViewBase<INewGitHubPaneViewModel, NewGitHubPaneView>
    {
    }

    [ExportViewFor(typeof(INewGitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewGitHubPaneView : GenericNewGitHubPaneView
    {
        [ImportingConstructor]
        public NewGitHubPaneView(INotificationDispatcher notifications)
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                infoPanel.Visibility = Visibility.Collapsed;
                d(notifications.Listen()
                    .ObserveOnDispatcher(DispatcherPriority.Normal)
                    .Subscribe(n =>
                    {
                        if (n.Type == Notification.NotificationType.Error || n.Type == Notification.NotificationType.Warning)
                            infoPanel.MessageType = MessageType.Warning;
                        else
                            infoPanel.MessageType = MessageType.Information;
                        infoPanel.Message = n.Message;
                    }));
            });
        }
    }
}
