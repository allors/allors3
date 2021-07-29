namespace Tests.Workspace
{
    using Allors.Workspace;

    public class PullResultAssert
    {
        private readonly IPullResult pullResult;

        public PullResultAssert(IPullResult pullResult) => this.pullResult = pullResult;

        public PullResultCollectionAssert<T> Collection<T>() where T : class, IObject => new PullResultCollectionAssert<T>(this.pullResult);

        public PullResultCollectionAssert<T> Collection<T>(string name) where T : class, IObject => new PullResultCollectionAssert<T>(this.pullResult, name);
    }
}
