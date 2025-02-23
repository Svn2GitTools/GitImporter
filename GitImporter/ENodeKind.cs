namespace GitImporter
{
    public enum ENodeKind
    {
        None,

        File, // "file"

        Directory, // "dir"

        Unknown, // not in dump file

        SymbolicLink, // not in dump file
    }
}
