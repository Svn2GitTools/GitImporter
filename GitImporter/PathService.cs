using GitImporter.Interfaces;

namespace GitImporter;

/// <summary>
/// Class PathService - Handles path-related operations
/// Implements the <see cref="IPathService" />
/// </summary>
/// <seealso cref="IPathService" />
internal class PathService : IPathService
{
    public string GetBranchOrTagNameForPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BranchTagService.DefaultBranchName;
        }

        if (path.StartsWith("trunk/", StringComparison.OrdinalIgnoreCase))
        {
            return BranchTagService.DefaultBranchName;
        }

        if (path.StartsWith("branches/", StringComparison.OrdinalIgnoreCase))
        {
            var branchNameForPath = path.Substring("branches/".Length).Split('/')[0];
            if (string.IsNullOrEmpty(branchNameForPath))
            {
                return BranchTagService.DefaultBranchName;
            }
            return branchNameForPath;
        }

        if (path.StartsWith("tags/", StringComparison.OrdinalIgnoreCase))
        {
            return "tags/" + path.Substring("tags/".Length).Split('/')[0];
        }

        return BranchTagService.DefaultBranchName;
    }

    public string GetRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BranchTagService.DefaultBranchName;
        }

        if (path.StartsWith("trunk/", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring("trunk/".Length);
        }
        
        if (path.StartsWith("branches/", StringComparison.OrdinalIgnoreCase))
        {
            int index = path.IndexOf('/', "branches/".Length);
            return index > 0 ? path.Substring(index + 1) : "";
        }

        if (path.StartsWith("tags/", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(path.IndexOf('/', "tags/".Length) + 1);
        }

        return BranchTagService.DefaultBranchName;
    }
}
