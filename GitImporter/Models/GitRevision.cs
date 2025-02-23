namespace GitImporter.Models;

public class GitRevision
{
    public string Author { get; set; }

    public List<GitNodeChange> Changes { get; } = new();

    public DateTime Date { get; set; }

    public string LogMessage { get; set; }

    public long Number { get; set; }

    public Dictionary<string, string> Properties { get; } = new();

   

    public void AddNode(GitNodeChange node)
    {
        Changes.Add(node);
    }
}
