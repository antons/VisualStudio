using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Extensions;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model that supports back/forward navigation of an inner content page.
    /// </summary>
    [Export(typeof(INavigationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NavigationViewModel : NewViewModelBase, INavigationViewModel
    {
        readonly ObservableAsPropertyHelper<INewPanePageViewModel> content;
        int index = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationViewModel"/> class.
        /// </summary>
        public NavigationViewModel()
        {
            History = new ReactiveList<INewPanePageViewModel>();

            var pos = this.WhenAnyValue(
                x => x.Index,
                x => x.History.Count,
                (i, c) => new { Index = i, Count = c });

            content = pos
                .Where(x => x.Index < x.Count)
                .Select(x => x.Index != -1 ? History[x.Index] : null)
                .StartWith((INewPanePageViewModel)null)
                .ToProperty(this, x => x.Content);

            NavigateBack = ReactiveCommand.Create(pos.Select(x => x.Index > 0));
            NavigateBack.Subscribe(_ => Back());
            NavigateForward = ReactiveCommand.Create(pos.Select(x => x.Index < x.Count - 1));
            NavigateForward.Subscribe(_ => Forward());
        }

        /// <inheritdoc/>
        public INewPanePageViewModel Content => content.Value;

        /// <inheritdoc/>
        public int Index
        {
            get { return index; }
            set { this.RaiseAndSetIfChanged(ref index, value); }
        }

        /// <inheritdoc/>
        public IReactiveList<INewPanePageViewModel> History { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> NavigateBack { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> NavigateForward { get; }

        /// <inheritdoc/>
        public void NavigateTo(INewPanePageViewModel page)
        {
            Guard.ArgumentNotNull(page, nameof(page));

            if (index < History.Count - 1)
            {
                History.RemoveRange(index + 1, History.Count - (index + 1));
            }

            History.Add(page);
            ++Index;
        }

        /// <inheritdoc/>
        public bool Back()
        {
            if (index == 0)
                return false;
            --Index;
            return true;
        }

        /// <inheritdoc/>
        public bool Forward()
        {
            if (index >= History.Count - 1)
                return false;
            ++Index;
            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Index = -1;
            History.Clear();
        }
    }
}