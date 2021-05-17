namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace;
    using Xunit;

    public class PullResultCollectionAssert<T> where T : IObject
    {
        private readonly T[] collection;

        public PullResultCollectionAssert(IPullResult pullResult) => this.collection = pullResult.GetCollection<T>();

        public void Single() => Assert.Single(this.collection);

        public void Equal(params string[] expected)
        {
            var actual = this.collection.Select(v => (string)((dynamic)v).Name);
            Assert.Equal(expected, actual);
        }
    }
}
