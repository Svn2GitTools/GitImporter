namespace GitImporter.Interfaces;

internal interface IGitConversionContext : IDisposable
{
    IGitRepository GitRepository { get; }
    ICommitService CommitService { get; }
}