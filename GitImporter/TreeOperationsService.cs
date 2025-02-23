using System.Text;

using GitImporter.Interfaces;

using LibGit2Sharp;

namespace GitImporter;

/// <summary>
/// Class TreeOperationsService - Handles file and directory operations in the Git tree
/// Implements the <see cref="ITreeOperationsService" />
/// </summary>
/// <seealso cref="ITreeOperationsService" />
internal class TreeOperationsService : ITreeOperationsService
{
    public void AddGitKeepFile(Repository repo, TreeDefinition treeDefinition, string directory)
    {
        // no .gitkeep on root
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }
        string gitKeepPath = CombineWithForwardSlash(directory, ".gitkeep");
        //string gitKeepPath = $"{directory}/.gitkeep";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("")))
        {
            Blob blob = repo.ObjectDatabase.CreateBlob(stream);
            if (directory != BranchTagService.DefaultBranchName)
            {
                treeDefinition.Add(gitKeepPath, blob, Mode.NonExecutableFile);
            }

            Console.WriteLine($"Added .gitkeep for directory: {directory}");
        }
    }

    public void MoveDirectory(
        Commit lastCommit,
        TreeDefinition treeDefinition,
        string oldPath,
        string newPath)
    {
        var oldEntry = lastCommit[oldPath];
        if (oldEntry == null)
        {
            Console.WriteLine($"Error: Subdirectory {oldPath} not found in the current state.");
            return;
        }

        var treeOld = oldEntry.Target as Tree;
        if (treeOld == null)
        {
            Console.WriteLine($"Error: {oldPath} is not a directory.");
            return;
        }

        treeDefinition.Remove(oldPath);
        treeDefinition.Add(newPath, treeOld);
    }

    public void MoveFile(
        Commit lastCommit,
        TreeDefinition treeDefinition,
        string oldPath,
        string newPath)
    {
        var oldEntry = lastCommit[oldPath];
        if (oldEntry == null)
        {
            Console.WriteLine($"Error: File {oldPath} not found in the current state.");
            return;
        }

        var blobOld = oldEntry.Target as Blob;
        if (blobOld == null)
        {
            Console.WriteLine($"Error: {oldPath} is not a file.");
            return;
        }

        treeDefinition.Remove(oldPath);
        treeDefinition.Add(newPath, blobOld, Mode.NonExecutableFile);
    }

    private static string CombineWithForwardSlash(string path1, string path2)
    {
        if (string.IsNullOrEmpty(path1))
        {
            return path2;
        }

        if (string.IsNullOrEmpty(path2))
        {
            return path1;
        }

        path1 = path1.TrimEnd('/', '\\');
        return path1.Length == 0 ? path2 : $"{path1}/{path2}";
    }
}
