using GitImporter.Interfaces;

using LibGit2Sharp;

namespace GitImporter;

public class GitRepository : IGitRepository
{
    private readonly string _gitRepoPath;

    private bool _disposed;

    public Repository Repository { get; }

    public GitRepository(string gitRepoPath, bool shouldInit)
    {
        _gitRepoPath = gitRepoPath;

        if (shouldInit)
        {
            try
            {
                Directory.CreateDirectory(_gitRepoPath);
                Repository =
                    new Repository(Repository.Init(_gitRepoPath)); // Initialize a new Git repo
                Console.WriteLine(
                    $"Created and initialized new Git repository at: {_gitRepoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error creating and initializing Git repository: {ex.Message}");
                throw; // Re-throw the exception to signal failure
            }
        }
        else
        {
            if (!Repository.IsValid(_gitRepoPath))
            {
                throw new ArgumentException(
                    $"The specified path '{_gitRepoPath}' is not a valid Git repository.");
            }

            Repository = new Repository(_gitRepoPath);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void UpdateBranchPointer(string branchName, Commit commit)
    {
        var branch = Repository.Branches[branchName];
        if (branch == null)
        {
            throw new ArgumentException($"Branch '{branchName}' not found.", nameof(branchName));
        }

        Repository.Refs.UpdateTarget(branch.Reference, commit.Id);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Repository?.Dispose();
            }

            _disposed = true;
        }
    }
}
