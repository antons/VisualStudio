using System;
using System.Reactive;
using System.Reactive.Subjects;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels.Dialog
{
    public class GitHubDialogWindowViewModelTests
    {
        public class TheInitializeMethod : TestBaseClass
        {
            [Fact]
            public void SetsContent()
            {
                var target = new GitHubDialogWindowViewModel();
                var content = Substitute.For<IDialogContentViewModel>();

                target.Initialize(content);

                Assert.Equal(content, target.Content);
            }

            [Fact]
            public void SignalsCloseWhenContentRaisesClosed()
            {
                var target = new GitHubDialogWindowViewModel();
                var content = Substitute.For<IDialogContentViewModel>();
                var closed = new Subject<Unit>();
                var signalled = false;

                content.Closed.Returns(closed);
                target.Closed.Subscribe(_ => signalled = true);
                target.Initialize(content);
                closed.OnNext(Unit.Default);

                Assert.True(signalled);
            }

            //[Fact]
            //public void UnsubscribesFromPreviousContentClosedEvent()
            //{
            //    var target = new GitHubDialogWindowViewModel();
            //    var content1 = Substitute.For<IDialogContentViewModel>();
            //    var content2 = Substitute.For<IDialogContentViewModel>();
            //    var signalled = false;

            //    target.Closed.Subscribe(_ => signalled = true);
            //    target.Initialize(content);
            //    content.Closed += Raise.Event();

            //    Assert.True(signalled);
            //}
        }
    }
}
