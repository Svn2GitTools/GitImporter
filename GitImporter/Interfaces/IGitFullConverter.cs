using GitImporter.Models;

namespace GitImporter.Interfaces;

public interface IGitFullConverter
{
    void Convert(List<GitRevision> revisions);
}