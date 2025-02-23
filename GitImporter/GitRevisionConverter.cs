using GitImporter.Interfaces;
using GitImporter.Models;

namespace GitImporter
{
    // Class for step-by-step conversion of revisions
    internal class GitRevisionConverter : IGitRevisionConverter
    {
        private readonly IGitConversionContext _context;

        public GitRevisionConverter(IGitConversionContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void ConvertRevision(GitRevision revision)
        {
            //Console.WriteLine($"Converting revision {revision.Number} to Git...");
            _context.CommitService.ConvertRevision(_context.GitRepository, revision);
            //Console.WriteLine($"Git conversion complete.");
        }
    }
}