using GitImporter.Interfaces;
using GitImporter.Models;

using LibGit2Sharp;

namespace GitImporter;

internal class CommitService : ICommitService
{
    private readonly IBranchTagService _branchTagService;
    private readonly IChangeProcessorService _changeProcessor;
    private readonly ICommitBuilderService _commitBuilder;
    private readonly IPathService _pathService;

    public CommitService(
        IBranchTagService branchTagService,
        IChangeProcessorService changeProcessor,
        ICommitBuilderService commitBuilder,
        IPathService pathService)
    {
        _branchTagService = branchTagService ?? throw new ArgumentNullException(nameof(branchTagService));
        _changeProcessor = changeProcessor ?? throw new ArgumentNullException(nameof(changeProcessor));
        _commitBuilder = commitBuilder ?? throw new ArgumentNullException(nameof(commitBuilder));
        _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
    }

    public void ConvertRevision(IGitRepository gitRepository, GitRevision revision)
    {
        Console.WriteLine($"Converting revision: {revision.Number}");

        string branchOrTagName = _pathService.GetBranchOrTagNameForPath(
            revision.Changes.FirstOrDefault()?.Path);

        // Determine the parent commit
        var targetBranch = gitRepository.Repository.Branches[branchOrTagName];
        var parentCommit = targetBranch?.Tip;

        // Process changes
        var commitChanges = _changeProcessor.ProcessChanges(
            gitRepository.Repository,
            revision,
            parentCommit?.Tree);

        if (!commitChanges.HasChanges && !commitChanges.HasTagAdditions)
        {
            Console.WriteLine($"No changes in revision {revision.Number}; skipping commit.");
            return;
        }

        if (commitChanges.HasTagDeletions && !commitChanges.HasChanges 
                                          && !commitChanges.HasTagAdditions)
        {
            _branchTagService.ProcessTagDeletions(gitRepository.Repository, commitChanges.TagsToDelete);
            Console.WriteLine($"Processed tag deletions for revision {revision.Number} without creating a commit.");
            return;
        }

        // Create new tree and commit
        Tree newTree = gitRepository.Repository.ObjectDatabase.CreateTree(commitChanges.TreeDefinition);
        Commit newCommit = _commitBuilder.CreateCommit(
            gitRepository.Repository,
            revision,
            newTree,
            parentCommit);

        // Update branches and tags
        _branchTagService.CreateOrUpdateBranch(gitRepository, branchOrTagName, newCommit);
        _branchTagService.ProcessTagAdditions(
            gitRepository.Repository,
            commitChanges.TagsToAdd,
            newCommit,
            revision.Number);
        _branchTagService.ProcessTagDeletions(
            gitRepository.Repository,
            commitChanges.TagsToDelete);
    }
}