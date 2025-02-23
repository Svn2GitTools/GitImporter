using LibGit2Sharp;

namespace GitImporter.Interfaces;

internal interface IGitRepository : IDisposable
{
    Repository Repository { get; }

    void UpdateBranchPointer(string branchName, Commit commit);
}