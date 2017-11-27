using System;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using Xunit;

public class NavigationViewModelTests
{
    public class TheContentProperty
    {
        [Fact]
        public void ContentShouldInitiallyBeNull()
        {
            var target = new NavigationViewModel();

            Assert.Null(target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingToPage()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            Assert.Equal(first, target.Content);

            target.NavigateTo(second);
            Assert.Same(second, target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingBack()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            Assert.Same(first, target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingForward()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.Forward();

            Assert.Same(second, target.Content);
        }

        [Fact]
        public void ContentShouldBeSetWhenReplacingFuture()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();
            var third = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.NavigateTo(third);

            Assert.Equal(third, target.Content);

            target.Back();

            Assert.Equal(first, target.Content);
        }
    }

    public class TheForwardAndBackCommands
    {
        [Fact]
        public void ForwardAndBackCommandsShouldInitiallyBeDisabled()
        {
            var target = new NavigationViewModel();

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
        public void ForwardAndBackCommandsShouldBeDisabledOnNavigatingToFirstPage()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
        public void BackCommandShouldBeEnabledOnNavigatingToSecondPage()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);

            Assert.True(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
        public void ForwardCommandShouldBeEnabledOnNavigatingBack()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.True(target.NavigateForward.CanExecute(null));
        }
    }

    public class TheClearMethod
    {
        [Fact]
        public void ClearClearsTheContentAndHistory()
        {
            var target = new NavigationViewModel();
            var first = Substitute.For<INewPanePageViewModel>();
            var second = Substitute.For<INewPanePageViewModel>();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Clear();

            Assert.Null(target.Content);
            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }
    }
}
