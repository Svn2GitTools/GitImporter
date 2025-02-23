using LibGit2Sharp;

namespace GitImporter.Interfaces;

internal interface IBranchTagService
{
    string GetBranchOrTagNameForPath(string path);
    string GetRelativePath(string path);
    void CreateOrUpdateBranch(IGitRepository gitRepository, string branchOrTagName, Commit newCommit);
    void ProcessTagAdditions(
        Repository repo,
        List<string> tagsToAdd,
        Commit newCommit,
        long revisionNumber);
    void ProcessTagDeletions(Repository repo, List<string> tagsToDelete);
}