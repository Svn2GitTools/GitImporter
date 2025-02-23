using System.Text;

using GitImporter.Interfaces;
using GitImporter.Models;

using LibGit2Sharp;

namespace GitImporter;

/// <summary>
/// Class ChangeProcessorService - Processes individual SVN changes
/// Implements the <see cref="IChangeProcessorService" />
/// </summary>
/// <seealso cref="IChangeProcessorService" />
internal class ChangeProcessorService : IChangeProcessorService
{
    private readonly IPathService _pathService;

    private readonly ITreeOperationsService _treeOperations;

    public ChangeProcessorService(IPathService pathService, ITreeOperationsService treeOperations)
    {
        _pathService = pathService;
        _treeOperations = treeOperations;
    }

    public CommitChanges ProcessChanges(Repository repo, GitRevision revision, Tree parentTree)
    {
        var treeDefinition =
            parentTree == null ? new TreeDefinition() : TreeDefinition.From(parentTree);
        var directoriesNeedingGitKeep = new HashSet<string>();
        var changes = new CommitChanges
                          {
                              TreeDefinition = treeDefinition,
                              TagsToAdd = new List<string>(),
                              TagsToDelete = new List<string>()
                          };

        foreach (var change in revision.Changes)
        {
            ProcessSingleChange(repo, change, treeDefinition, directoriesNeedingGitKeep, changes);
        }

        // Add .gitkeep files
        foreach (var dir in directoriesNeedingGitKeep)
        {
            _treeOperations.AddGitKeepFile(repo, treeDefinition, dir);
            changes.HasChanges = true;
        }

        // Add placeholder for tag-only changes
        if (!changes.HasChanges && changes.HasTagAdditions)
        {
            AddPlaceholderFile(repo, treeDefinition, revision.Number);
            changes.HasChanges = true;
        }

        return changes;
    }

    private void AddPlaceholderFile(
        Repository repo,
        TreeDefinition treeDefinition,
        long revisionNumber)
    {
        string placeholderPath = $".placeholder-r{revisionNumber}";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("")))
        {
            Blob blob = repo.ObjectDatabase.CreateBlob(stream);
            treeDefinition.Add(placeholderPath, blob, Mode.NonExecutableFile);
            Console.WriteLine($"Added placeholder file for tag operations: {placeholderPath}");
        }
    }

    private Blob CreateBlob(Repository repo, GitNodeChange change)
    {
        if (change.IsBinary && change.BinaryContent != null)
        {
            using (var stream = new MemoryStream(change.BinaryContent))
            {
                return repo.ObjectDatabase.CreateBlob(stream);
            }
        }

        if (!string.IsNullOrEmpty(change.TextContent))
        {
            var encoding = Encoding.UTF8;
            using (var stream = new MemoryStream(encoding.GetBytes(change.TextContent)))
            {
                return repo.ObjectDatabase.CreateBlob(stream);
            }
        }

        return null;
    }

    private void ProcessAddOrModify(
        Repository repo,
        GitNodeChange change,
        TreeDefinition treeDefinition,
        HashSet<string> directoriesNeedingGitKeep,
        CommitChanges changes,
        string branchOrTagName,
        string relativePath)
    {
        var lastCommit = repo.Head.Tip;

        if (change.Kind == ENodeKind.Directory)
        {
            ProcessDirectoryChange(
                lastCommit,
                change,
                treeDefinition,
                directoriesNeedingGitKeep,
                changes,
                branchOrTagName,
                relativePath);

            return;
        }

        Blob blob = CreateBlob(repo, change);
        if (blob != null)
        {
            treeDefinition.Add(relativePath, blob, Mode.NonExecutableFile);
            changes.HasChanges = true;

            // Remove parent directory from gitkeep if it has files
            RemoveParentDirectoryFromGitKeep(relativePath, directoriesNeedingGitKeep);
        }
        else if (change.CopyFromPath != null)
        {
            var copyFromPath = _pathService.GetRelativePath(change.CopyFromPath);
            _treeOperations.MoveFile(lastCommit, treeDefinition, copyFromPath, relativePath);
        }
    }

    private void ProcessDelete(
        TreeDefinition treeDefinition,
        CommitChanges changes,
        string branchOrTagName,
        string relativePath)
    {
        if (branchOrTagName.StartsWith("tags/"))
        {
            string tagName = branchOrTagName.Substring("tags/".Length);
            changes.TagsToDelete.Add(tagName);
            changes.HasTagDeletions = true;
        }
        else
        {
            treeDefinition.Remove(relativePath);
            changes.HasChanges = true;
        }
    }

    private void ProcessDirectoryChange(
        Commit lastCommit,
        GitNodeChange change,
        TreeDefinition treeDefinition,
        HashSet<string> directoriesNeedingGitKeep,
        CommitChanges changes,
        string branchOrTagName,
        string relativePath)
    {
        if (!branchOrTagName.StartsWith("tags/"))
        {
            if (change.CopyFromPath != null)
            {
                var relativeCopyFromPath = _pathService.GetRelativePath(change.CopyFromPath);
                _treeOperations.MoveDirectory(
                    lastCommit,
                    treeDefinition,
                    relativeCopyFromPath,
                    relativePath);
            }
            else
            {
                directoriesNeedingGitKeep.Add(relativePath);
                // Remove parent directory from gitkeep if it has directory
                RemoveParentDirectoryFromGitKeep(relativePath, directoriesNeedingGitKeep);
            }
        }
        else
        {
            // Track tag additions
            string tagName = branchOrTagName.Substring("tags/".Length);
            if (!string.IsNullOrEmpty(tagName))
            {
                changes.TagsToAdd.Add(tagName);
                changes.HasTagAdditions = true;
            }
        }
    }

    private void ProcessSingleChange(
        Repository repo,
        GitNodeChange change,
        TreeDefinition treeDefinition,
        HashSet<string> directoriesNeedingGitKeep,
        CommitChanges changes)
    {
        var normalizedPath = change.Path.Replace('\\', '/');
        string branchOrTagName = _pathService.GetBranchOrTagNameForPath(normalizedPath);
        string relativePath = _pathService.GetRelativePath(normalizedPath);

        switch (change.Action)
        {
            case EChangeAction.Add:
            case EChangeAction.Modify:
                ProcessAddOrModify(
                    repo,
                    change,
                    treeDefinition,
                    directoriesNeedingGitKeep,
                    changes,
                    branchOrTagName,
                    relativePath);
                break;

            case EChangeAction.Delete:
                ProcessDelete(treeDefinition, changes, branchOrTagName, relativePath);
                break;

            default:
                // Optionally handle unexpected or unhandled enum values.
                break;
        }
    }

    private void RemoveParentDirectoryFromGitKeep(
        string relativePath,
        HashSet<string> directoriesNeedingGitKeep)
    {
        string parentDir = Path.GetDirectoryName(relativePath);
        if (parentDir != null)
        {
            string normalizedParentDir = parentDir.Replace('\\', '/');
            directoriesNeedingGitKeep.RemoveWhere(
                dir =>
                    {
                        var normalizedDir = dir.Replace('\\', '/');
                        return normalizedDir.Equals(
                            normalizedParentDir,
                            StringComparison.OrdinalIgnoreCase);
                    });
        }
    }
}
