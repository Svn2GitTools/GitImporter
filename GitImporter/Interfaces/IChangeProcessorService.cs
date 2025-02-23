using GitImporter.Models;

using LibGit2Sharp;

namespace GitImporter.Interfaces;

internal interface IChangeProcessorService
{
    CommitChanges ProcessChanges(Repository repo, GitRevision revision, Tree parentTree);
}