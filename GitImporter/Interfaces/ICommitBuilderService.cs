using GitImporter.Models;

using LibGit2Sharp;

namespace GitImporter.Interfaces;

internal interface ICommitBuilderService
{
    Commit CreateCommit(Repository repo, GitRevision revision, Tree newTree, Commit parentCommit);
}