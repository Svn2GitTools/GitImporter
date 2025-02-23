using GitImporter.Interfaces;
using GitImporter.Models;

namespace GitImporter;

public class GitFullConverter : IGitFullConverter
{
    private readonly IGitRevisionConverter _revisionConverter;

    public GitFullConverter(IGitRevisionConverter revisionConverter)
    {
        _revisionConverter = revisionConverter ?? throw new ArgumentNullException(nameof(revisionConverter));
    }

    public void Convert(List<GitRevision> revisions)
    {
        Console.WriteLine("Converting all revisions to Git...");

        foreach (var revision in revisions)
        {
            _revisionConverter.ConvertRevision(revision); // Reuse the step-by-step conversion logic
        }

        Console.WriteLine("Git conversion complete.");
    }
}