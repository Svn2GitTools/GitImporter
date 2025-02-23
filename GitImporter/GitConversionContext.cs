using GitImporter.Interfaces;

namespace GitImporter;

internal class GitConversionContext : IGitConversionContext
{
    public IGitRepository GitRepository { get; }
    public ICommitService CommitService { get; }
    private bool _disposed = false;

    public GitConversionContext(IGitRepository gitRepository, ICommitService commitService)
    {
        GitRepository = gitRepository ?? throw new ArgumentNullException(nameof(gitRepository));
        CommitService = commitService ?? throw new ArgumentNullException(nameof(commitService));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                GitRepository?.Dispose();
            }

            _disposed = true;
        }
    }
}