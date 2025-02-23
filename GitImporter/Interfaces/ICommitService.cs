using GitImporter.Models;

namespace GitImporter.Interfaces;

internal interface ICommitService
{
    void ConvertRevision(IGitRepository gitRepository, GitRevision revision);
}