namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace;
    using Allors.Workspace.Signals;
    using Xunit;

    public class PullResultCollectionAssert<T> where T : class, IObject
    {
        private readonly T[] collection;

        public PullResultCollectionAssert(IPullResult pullResult, string name = null) => this.collection = name != null ? pullResult.GetCollection<T>(name) : pullResult.GetCollection<T>();

        public void Single() => Assert.Single(this.collection);

        public void Equal(params string[] expected)
        {
            // Order-insensitive: an unsorted Pull/Extent has no defined row order, and it differs across
            // adapters (e.g. PostgreSQL vs SQL Server). Compare as sets by sorting both sides.
            var actual = this.collection.Select(v => ((IRoleSignal<string>)((dynamic)v).Name).Value);
            Assert.Equal(expected.OrderBy(v => v), actual.OrderBy(v => v));
        }
    }
}
