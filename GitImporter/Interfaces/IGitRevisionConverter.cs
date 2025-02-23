using GitImporter.Models;

namespace GitImporter.Interfaces;

public interface IGitRevisionConverter
{
    void ConvertRevision(GitRevision revision);
}