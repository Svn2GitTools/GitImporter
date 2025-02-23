namespace GitImporter.Interfaces;

internal interface IGitConversionContextFactory
{
    IGitConversionContext Create(string gitRepoPath);
}