using GitImporter.Interfaces;

using LibGit2Sharp;

namespace GitImporter;

internal class BranchTagService : IBranchTagService
{
    public const string DefaultBranchName = "master";

    public string GetBranchOrTagNameForPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return DefaultBranchName;
        }

        if (path.StartsWith("trunk/", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultBranchName;
        }

        if (path.StartsWith("branches/", StringComparison.OrdinalIgnoreCase))
        {
            // Extract branch name after "branches/"
            return path.Substring("branches/".Length).Split('/')[0];
        }

        if (path.StartsWith("tags/", StringComparison.OrdinalIgnoreCase))
        {
            // Extract tag name after "tags/"
            return "tags/" + path.Substring("tags/".Length).Split('/')[0];
        }

        return DefaultBranchName;
    }

    public string GetRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (path.StartsWith("trunk/", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring("trunk/".Length);
        }

        if (path.StartsWith("branches/", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(path.IndexOf('/', "branches/".Length) + 1);
        }

        if (path.StartsWith("tags/", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(path.IndexOf('/', "tags/".Length) + 1);
        }

        return path;
    }

    public void CreateOrUpdateBranch(IGitRepository gitRepository, string branchOrTagName, Commit newCommit)
    {
        if (branchOrTagName == null)
        {
            branchOrTagName = DefaultBranchName;
        }

        try
        {
            // Check if this is a tag or a branch
            if (branchOrTagName.StartsWith("tags/"))
            {
                // No action needed here. Tags are handled in ProcessTagAdditions
            }
            else
            {
                // Create or update a Git branch
                Branch targetBranch =
                    gitRepository.Repository.Branches.FirstOrDefault(b => b.FriendlyName == branchOrTagName);
                if (targetBranch == null)
                {
                    targetBranch = gitRepository.Repository.CreateBranch(branchOrTagName, newCommit);
                }

                //gitRepository.Repository.Refs.UpdateTarget("HEAD", newCommit.Sha);
                gitRepository.UpdateBranchPointer(branchOrTagName, newCommit);
                Console.WriteLine(
                    $"Commit created on branch {branchOrTagName} with SHA {newCommit.Sha}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during branch or tag creation/update: {ex.Message}");
        }
    }

    public void ProcessTagAdditions(
        Repository repo,
        List<string> tagsToAdd,
        Commit newCommit,
        long revisionNumber)
    {
        foreach (var tagName in tagsToAdd)
        {
            string finalTagName = tagName;
            bool tagExists = repo.Tags.Any(t => t.FriendlyName == tagName);

            if (tagExists)
            {
                // Check if the existing tag points to the same commit
                var existingTag = repo.Tags[tagName];
                if (existingTag.Target is Commit taggedCommit && taggedCommit.Sha == newCommit.Sha)
                {
                    Console.WriteLine($"Tag '{tagName}' already exists on the target commit. Skipping.");
                    continue;
                }

                // Add letters to the revision number
                string baseTagName = $"{tagName}-r{revisionNumber}";
                char letter = 'a';
                while (repo.Tags.Any(t => t.FriendlyName == $"{baseTagName}{letter}"))
                {
                    letter++;
                    if (letter > 'z')
                    {
                        throw new InvalidOperationException($"Exceeded maximum number of revisions for tag '{tagName}'");
                    }
                }

                finalTagName = $"{baseTagName}{letter}";
                Console.WriteLine($"Tag '{tagName}' already exists; using unique name: {finalTagName}");
            }

            repo.Tags.Add(finalTagName, newCommit);
            Console.WriteLine($"Tag '{finalTagName}' created at commit {newCommit.Sha}");
        }
    }


    public void ProcessTagDeletions(LibGit2Sharp.Repository repo, List<string> tagsToDelete)
    {
        foreach (var tagName in tagsToDelete)
        {
            if (repo.Tags[tagName] != null)
            {
                repo.Tags.Remove(tagName);
                Console.WriteLine($"Deleted tag: {tagName}");
            }
        }
    }
}