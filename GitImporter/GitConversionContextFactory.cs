using GitImporter.Interfaces;

namespace GitImporter;

internal class GitConversionContextFactory : IGitConversionContextFactory
{
    private readonly IGitRepositoryFactory _gitRepositoryFactory;
    private readonly ICommitService _commitService;

    public GitConversionContextFactory(IGitRepositoryFactory gitRepositoryFactory, ICommitService commitService)
    {
        _gitRepositoryFactory = gitRepositoryFactory ?? throw new ArgumentNullException(nameof(gitRepositoryFactory));
        _commitService = commitService ?? throw new ArgumentNullException(nameof(commitService));
    }

    public IGitConversionContext Create(string gitRepoPath)
    {
        return new GitConversionContext(_gitRepositoryFactory.GetRepository(gitRepoPath), _commitService);
    }
}