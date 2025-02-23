namespace GitImporter.Interfaces;

internal interface IGitRepositoryFactory
{
    IGitRepository GetRepository(string gitRepoPath);
}