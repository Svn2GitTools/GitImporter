using LibGit2Sharp;

namespace GitImporter.Interfaces;

internal interface ITreeOperationsService
{
    void MoveFile(Commit lastCommit, TreeDefinition treeDefinition, string oldPath, string newPath);
    void MoveDirectory(Commit lastCommit, TreeDefinition treeDefinition, string oldPath, string newPath);
    void AddGitKeepFile(Repository repo, TreeDefinition treeDefinition, string directory);
}