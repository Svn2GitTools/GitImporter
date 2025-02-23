using GitImporter.Interfaces;

namespace GitImporter;

internal class GitRepositoryFactory : IGitRepositoryFactory
{
    public IGitRepository GetRepository(string gitRepoPath)
    {
        bool shouldInit = !Directory.Exists(gitRepoPath); // Check to initialize

        return new GitRepository(gitRepoPath, shouldInit);
    }
}