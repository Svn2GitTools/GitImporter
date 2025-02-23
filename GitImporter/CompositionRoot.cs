using GitImporter.Interfaces;

namespace GitImporter
{
    public class CompositionRoot : IDisposable
    {
        private IBranchTagService BranchTagService { get; }

        private ICommitService CommitService { get; }

        private IGitConversionContextFactory ConversionContextFactory { get; }

        private GitConversionManager ConversionManager { get; }

        private IGitRepositoryFactory GitRepositoryFactory { get; }

        public CompositionRoot(string gitRepoPath, IAuthorsMap? authorsMap)
        {
            BranchTagService = new BranchTagService();
            IPathService pathService = new PathService();
            ITreeOperationsService treeOperationsService = new TreeOperationsService();
            ChangeProcessorService changeProcessorService =
                new ChangeProcessorService(pathService, treeOperationsService);
            ICommitBuilderService commitBuilder = new CommitBuilderService(authorsMap);
            CommitService = new CommitService(
                BranchTagService,
                changeProcessorService,
                commitBuilder,
                pathService);
            GitRepositoryFactory = new GitRepositoryFactory();
            ConversionContextFactory = new GitConversionContextFactory(
                GitRepositoryFactory,
                CommitService);

            // Pass gitRepoPath to ConversionManager for proper initialization
            ConversionManager = new GitConversionManager(ConversionContextFactory, gitRepoPath);
        }

        public void Checkout()
        {
            ConversionManager.Checkout();
        }

        public IGitFullConverter CreateFullConverter()
        {
            return new GitFullConverter(ConversionManager.RevisionConverter);
        }

        public void Dispose()
        {
            ConversionManager.Dispose();
        }

        public IGitRevisionConverter GetRevisionConverter()
        {
            return ConversionManager.RevisionConverter;
        }
    }
}
