using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.UI;
using GitHub.Validation;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(INewLoginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NewLoginViewModel : NewViewModelBase, INewLoginViewModel
    {
        readonly ILoginCredentialsViewModel credentials;
        readonly ILogin2FaViewModel twoFactor;
        readonly IDelegatingTwoFactorChallengeHandler twoFactorHandler;
        IDialogContentViewModel content;
        ObservableAsPropertyHelper<string> title;

        [ImportingConstructor]
        public NewLoginViewModel(
            ILoginCredentialsViewModel credentials,
            ILogin2FaViewModel twoFactor,
            IDelegatingTwoFactorChallengeHandler twoFactorHandler)
        {
            this.credentials = credentials;
            this.twoFactor = twoFactor;
            this.twoFactorHandler = twoFactorHandler;

            twoFactorHandler.SetViewModel(new TwoFactorHandlerThunk(this));

            content = credentials;
            title = this.WhenAny(x => x.Content, x => x.Value.Title).ToProperty(this, x => x.Title);
            Closed = credentials.Closed;            

            twoFactor.WhenAnyValue(x => x.TwoFactorType)
                .Subscribe(x =>
                {
                    if (x == TwoFactorType.None)
                    {
                        Content = credentials;
                    }
                    else
                    {
                        Content = twoFactor;
                    }
                });
        }

        public string Title => title.Value;

        public IDialogContentViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        public IObservable<Unit> Closed { get; }

        //// --------------------------------------------------------
        //// TODO: Sort this out before merging the MVVM refactor!!!!
        //// --------------------------------------------------------
        class TwoFactorHandlerThunk : ITwoFactorDialogViewModel
        {
            readonly NewLoginViewModel parent;

            public TwoFactorHandlerThunk(NewLoginViewModel parent)
            {
                this.parent = parent;
                Cancel = ReactiveCommand.Create();
                Cancel.Subscribe(x => parent.twoFactor.Cancel());
            }

            public string AuthenticationCode { get; set; }
            public ReactivePropertyValidator AuthenticationCodeValidator { get; }
            public ReactiveCommand<object> Cancel { get; }
            public string Description { get; }
            public IObservable<Unit> Done { get; }
            public bool InvalidAuthenticationCode { get; }
            public bool IsAuthenticationCodeSent { get; }
            public bool IsBusy { get; }
            public bool IsShowing { get; }
            public bool IsSms { get; }
            public ReactiveCommand<object> NavigateLearnMore { get; }
            public ReactiveCommand<object> OkCommand { get; }
            public ReactiveCommand<object> ResendCodeCommand { get; }
            public bool ShowErrorMessage { get; }
            public string Title { get; }
            IObservable<Unit> IHasCancel.Cancel { get; }
            public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

            public void Initialize(ViewWithData data)
            {
            }

            public IObservable<TwoFactorChallengeResult> Show(UserError error)
            {
                return parent.twoFactor.Show(error);
            }
        }
    }
}
