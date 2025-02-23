using System.Globalization;

using GitImporter.Interfaces;
using GitImporter.Models;

using LibGit2Sharp;

namespace GitImporter;

/// <summary>
/// Class CommitBuilderService - Handles commit creation and metadata
/// Implements the <see cref="ICommitBuilderService" />
/// </summary>
/// <seealso cref="ICommitBuilderService" />
public class CommitBuilderService : ICommitBuilderService
{
    private readonly IAuthorsMap? _authorsMap;

    public CommitBuilderService(IAuthorsMap? authorsMap)
    {
        _authorsMap = authorsMap;
    }
    public Commit CreateCommit(
        Repository repo,
        GitRevision revision,
        Tree newTree,
        Commit parentCommit)
    {
        var commitTime = GetCommitTime(revision);
        string revisionAuthor = revision.Author ?? "unknown";
        
        var author = CreateSignature(revisionAuthor, commitTime);
        var message = revision.LogMessage ?? "Imported from SVN";
        var parents = parentCommit != null ? new[] { parentCommit } : Array.Empty<Commit>();

        return repo.ObjectDatabase.CreateCommit(
            author,
            author, // Using same signature for author and committer
            message,
            newTree,
            parents,
            false);
    }

    private Signature CreateSignature(string author, DateTimeOffset commitTime)
    {
        var email = $"{author}@example.com";
        if (_authorsMap != null)
        {
            email = _authorsMap.GetAuthorEmail(author);
        }
        return new Signature(author, email, commitTime);
    }

    private DateTimeOffset GetCommitTime(GitRevision revision)
    {
        if (revision.Properties.ContainsKey("svn:date") && revision.Number == 0)
        {
            if (DateTimeOffset.TryParseExact(
                    revision.Properties["svn:date"],
                    "yyyy-MM-ddTHH:mm:ss.ffffffZ",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var commitTime))
            {
                return commitTime;
            }

            Console.WriteLine(
                $"svn:date value '{revision.Properties["svn:date"]}' does not match the expected format. Using Revision.Date instead.");
        }

        return revision.Date;
    }
}
