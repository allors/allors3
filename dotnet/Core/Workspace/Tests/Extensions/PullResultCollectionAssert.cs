namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using Allors.Workspace;
    using Xunit;

    public class PullResultCollectionAssert<T> where T : class, IObject
    {
        private readonly T[] collection;

        public PullResultCollectionAssert(IPullResult pullResult, string name = null) => this.collection = name != null ? pullResult.GetCollection<T>(name) : pullResult.GetCollection<T>();

        public void Single() => Assert.Single(this.collection);

        /// <summary>
        /// Asserts that the collection holds exactly the objects with these names, in any order. A pull
        /// without sorting makes no promise about the order, and databases return the same objects in
        /// different orders; a test that pulls with sorting asserts the order on the array itself.
        /// </summary>
        public void Equal(params string[] expected)
        {
            var actual = this.collection.Select(v => (string)((dynamic)v).Name);
            Assert.Equal(expected.OrderBy(v => v, StringComparer.Ordinal), actual.OrderBy(v => v, StringComparer.Ordinal));
        }
    }
}
