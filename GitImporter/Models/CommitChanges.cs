using LibGit2Sharp;

namespace GitImporter.Models;

internal class CommitChanges
{
    public TreeDefinition TreeDefinition { get; set; }
    public bool HasChanges { get; set; }
    public bool HasTagAdditions { get; set; }
    public bool HasTagDeletions { get; set; }
    public List<string> TagsToAdd { get; set; } = new List<string>();
    public List<string> TagsToDelete { get; set; } = new List<string>();
}