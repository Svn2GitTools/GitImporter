namespace GitImporter.Interfaces;

internal interface IPathService
{
    string GetBranchOrTagNameForPath(string path);
    string GetRelativePath(string path);
}