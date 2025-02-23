using GitImporter;
using GitImporter.Interfaces;
using GitImporter.Models;

namespace GitImporterDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Git repo creator");
            string gitRepoPath = "C:/GitDemoRepository"; // Specify where to create/use the Git repo
            // TODO: You might need to initialize an empty Git repository at gitRepoPath beforehand

            // Example Author Mapping
            IAuthorsMap authorsMap = new MyAuthorsMap(); // Create your own implementation

            using (var compositionRoot = new CompositionRoot(gitRepoPath, authorsMap))
            {
                var revisionConverter = compositionRoot.GetRevisionConverter();

                // Simulate processing some data source and creating GitRevisions
                List<GitRevision> revisionsToImport = GenerateRevisionsFromDataSource();

                foreach (var gitRevision in revisionsToImport)
                {
                    revisionConverter.ConvertRevision(gitRevision);
                }

                compositionRoot.Checkout(); // Finalize Git repository
            }

            Console.WriteLine("Git history import complete!");
        }

        // Example:  You would replace this with your actual data processing logic
        static List<GitRevision> GenerateRevisionsFromDataSource()
        {
            var revisions = new List<GitRevision>();

            // Example Revision 1
            var revision1 = new GitRevision
            {
                Number = 1,
                Author = "user1", // SVN username or similar identifier
                Date = DateTime.Now.AddDays(-2),
                LogMessage = "Initial commit",
            };
            revision1.AddNode(new GitNodeChange { Action = EChangeAction.Add, Kind = ENodeKind.File, Path = "file1.txt", TextContent = "Content of file1" });
            revisions.Add(revision1);

            // Example Revision 2
            var revision2 = new GitRevision
            {
                Number = 2,
                Author = "user2", // SVN username or similar identifier
                Date = DateTime.Now.AddDays(-1),
                LogMessage = "Modify file1 and add file2",
            };
            revision2.AddNode(new GitNodeChange { Action = EChangeAction.Modify, Kind = ENodeKind.File, Path = "file1.txt", TextContent = "Updated content of file1" });
            revision2.AddNode(new GitNodeChange { Action = EChangeAction.Add, Kind = ENodeKind.File, Path = "file2.txt", TextContent = "Content of file2" });
            revisions.Add(revision2);

            return revisions;
        }

        // Example IAuthorsMap implementation (replace with your actual mapping logic)
        class MyAuthorsMap : IAuthorsMap
        {
            public string GetAuthorEmail(string authorName)
            {
                // Implement your author mapping logic here.
                // For example, based on authorName, return the corresponding Git email.
                if (authorName == "user1") return "user1@example.com";
                if (authorName == "user2") return "user2@example.com";
                return $"{authorName}@example.com"; // Default email if no mapping found
            }
        }
    }
}
