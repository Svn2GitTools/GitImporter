using GitImporter.Interfaces;

using LibGit2Sharp;

namespace GitImporter;

internal class GitConversionManager : IDisposable
{
    private readonly IGitConversionContext _context;

    public IGitRevisionConverter RevisionConverter { get; } // Use the interface

    public GitConversionManager(
        IGitConversionContextFactory conversionContextFactory,
        string gitRepoPath)
    {
        _context = conversionContextFactory.Create(gitRepoPath);
        RevisionConverter =
            new GitRevisionConverter(_context); // Assign the implementation to the interface
    }

    public void Checkout()
    {
        try
        {
            Commands.Checkout(
                _context.GitRepository.Repository,
                _context.GitRepository.Repository.Branches[BranchTagService.DefaultBranchName]);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: '{e.Message}'");
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
