﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying details of a pull request.
    /// </summary>
    public interface INewPullRequestDetailViewModel : INewPanePageViewModel, IOpenInBrowser
    {
        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        IPullRequestModel Model { get; }

        /// <summary>
        /// Gets the session for the pull request.
        /// </summary>
        IPullRequestSession Session { get; }

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the owner of the remote repository that contains the pull request.
        /// </summary>
        /// <remarks>
        /// The remote repository may be different from the local repository if the local
        /// repository is a fork and the user is viewing pull requests from the parent repository.
        /// </remarks>
        string RemoteRepositoryOwner { get; }

        /// <summary>
        /// Gets the Pull Request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        string TargetBranchDisplayName { get; }

        /// <summary>
        /// Gets the number of comments made on the pull request.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request branch is checked out.
        /// </summary>
        bool IsCheckedOut { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request comes from a fork.
        /// </summary>
        bool IsFromFork { get; }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        IReadOnlyList<IPullRequestChangeNode> ChangedFilesTree { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Checkout"/> command.
        /// </summary>
        IPullRequestCheckoutState CheckoutState { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Pull"/> and <see cref="Push"/> commands.
        /// </summary>
        IPullRequestUpdateState UpdateState { get; }

        /// <summary>
        /// Gets the error message to be displayed below the checkout button.
        /// </summary>
        string OperationError { get; }

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        ReactiveCommand<Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that pulls changes to the current branch.
        /// </summary>
        ReactiveCommand<Unit> Pull { get; }

        /// <summary>
        /// Gets a command that pushes changes from the current branch.
        /// </summary>
        ReactiveCommand<Unit> Push { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between BASE and HEAD.
        /// </summary>
        ReactiveCommand<object> DiffFile { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between the version in
        /// the working directory and HEAD.
        /// </summary>
        ReactiveCommand<object> DiffFileWithWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> from disk.
        /// </summary>
        ReactiveCommand<object> OpenFileInWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> as it appears in the PR.
        /// </summary>
        ReactiveCommand<object> ViewFile { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="number">The pull request number.</param>
        Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int number);

        /// <summary>
        /// Gets a file as it appears in the pull request.
        /// </summary>
        /// <param name="file">The changed file.</param>
        /// <param name="head">
        /// If true, gets the file at the PR head, otherwise gets the file at the PR merge base.
        /// </param>
        /// <returns>The path to a temporary file.</returns>
        Task<string> ExtractFile(IPullRequestFileNode file, bool head);

        /// <summary>
        /// Gets the full path to a file in the working directory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The full path to the file in the working directory.</returns>
        string GetLocalFilePath(IPullRequestFileNode file);
    }
}
