// <copyright file="ExtentTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Linq;

    using Allors;
    using Domain;
    using Xunit;

    public class ExtentTest : Adapters.ExtentTest, IClassFixture<Fixture<ExtentTest>>
    {
        private readonly Profile profile;

        public ExtentTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();

        [Fact]
        public override void SortOne()
        {
            foreach (var init in this.Inits)
            {
                foreach (var marker in this.Markers)
                {
                    init();
                    this.Populate();
                    var m = this.Transaction.Database.Context().M;

                    this.Transaction.Commit();

                    this.c1B.C1AllorsString = "3";
                    this.c1C.C1AllorsString = "1";
                    this.c1D.C1AllorsString = "2";

                    this.Transaction.Commit();

                    marker();

                    var extent = this.Transaction.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString);

                    var sortedObjects = (C1[])extent.ToArray(typeof(C1));
                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(this.c1A, sortedObjects[0]);
                    Assert.Equal(this.c1C, sortedObjects[1]);
                    Assert.Equal(this.c1D, sortedObjects[2]);
                    Assert.Equal(this.c1B, sortedObjects[3]);

                    marker();

                    extent = this.Transaction.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                    sortedObjects = (C1[])extent.ToArray(typeof(C1));
                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(this.c1A, sortedObjects[0]);
                    Assert.Equal(this.c1C, sortedObjects[1]);
                    Assert.Equal(this.c1D, sortedObjects[2]);
                    Assert.Equal(this.c1B, sortedObjects[3]);

                    marker();

                    extent = this.Transaction.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                    sortedObjects = (C1[])extent.ToArray(typeof(C1));
                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(this.c1A, sortedObjects[0]);
                    Assert.Equal(this.c1C, sortedObjects[1]);
                    Assert.Equal(this.c1D, sortedObjects[2]);
                    Assert.Equal(this.c1B, sortedObjects[3]);

                    marker();

                    extent = this.Transaction.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);

                    sortedObjects = (C1[])extent.ToArray(typeof(C1));
                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(this.c1B, sortedObjects[0]);
                    Assert.Equal(this.c1D, sortedObjects[1]);
                    Assert.Equal(this.c1C, sortedObjects[2]);
                    Assert.Equal(this.c1A, sortedObjects[3]);

                    marker();

                    extent = this.Transaction.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);

                    sortedObjects = (C1[])extent.ToArray(typeof(C1));
                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(this.c1B, sortedObjects[0]);
                    Assert.Equal(this.c1D, sortedObjects[1]);
                    Assert.Equal(this.c1C, sortedObjects[2]);
                    Assert.Equal(this.c1A, sortedObjects[3]);

                    foreach (var useOperator in this.UseOperator)
                    {
                        if (useOperator)
                        {
                            marker();

                            var firstExtent = this.Transaction.Extent(m.C1);
                            firstExtent.Filter.AddLike(m.C1.C1AllorsString, "1");
                            var secondExtent = this.Transaction.Extent(m.C1);
                            extent = this.Transaction.Union(firstExtent, secondExtent);
                            secondExtent.Filter.AddLike(m.C1.C1AllorsString, "3");
                            extent.AddSort(m.C1.C1AllorsString);

                            sortedObjects = (C1[])extent.ToArray(typeof(C1));
                            Assert.Equal(2, sortedObjects.Length);
                            Assert.Equal(this.c1C, sortedObjects[0]);
                            Assert.Equal(this.c1B, sortedObjects[1]);
                        }
                    }
                }
            }
        }

        [Fact]
        public override void SortTwo()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                this.Populate();

                this.c1B.C1AllorsString = "a";
                this.c1C.C1AllorsString = "b";
                this.c1D.C1AllorsString = "a";

                this.c1B.C1AllorsInteger = 2;
                this.c1C.C1AllorsInteger = 1;
                this.c1D.C1AllorsInteger = 0;

                this.Transaction.Commit();

                var extent = this.Transaction.Extent(m.C1);
                extent.AddSort(m.C1.C1AllorsString);
                extent.AddSort(m.C1.C1AllorsInteger);

                var sortedObjects = (C1[])extent.ToArray(typeof(C1));
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1D, sortedObjects[1]);
                Assert.Equal(this.c1B, sortedObjects[2]);
                Assert.Equal(this.c1C, sortedObjects[3]);

                extent = this.Transaction.Extent(m.C1);
                extent.AddSort(m.C1.C1AllorsString);
                extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Ascending);

                sortedObjects = (C1[])extent.ToArray(typeof(C1));
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1D, sortedObjects[1]);
                Assert.Equal(this.c1B, sortedObjects[2]);
                Assert.Equal(this.c1C, sortedObjects[3]);

                extent = this.Transaction.Extent(m.C1);
                extent.AddSort(m.C1.C1AllorsString);
                extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Descending);

                sortedObjects = (C1[])extent.ToArray(typeof(C1));
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1B, sortedObjects[1]);
                Assert.Equal(this.c1D, sortedObjects[2]);
                Assert.Equal(this.c1C, sortedObjects[3]);

                extent = this.Transaction.Extent(m.C1);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);
                extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Descending);

                sortedObjects = (C1[])extent.ToArray(typeof(C1));
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1C, sortedObjects[0]);
                Assert.Equal(this.c1B, sortedObjects[1]);
                Assert.Equal(this.c1D, sortedObjects[2]);
                Assert.Equal(this.c1A, sortedObjects[3]);
            }
        }

        [Fact]
        public override void SortDifferentTransaction()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);
                var c1B = C1.Create(this.Transaction);
                var c1C = C1.Create(this.Transaction);
                var c1D = C1.Create(this.Transaction);

                c1A.C1AllorsString = "2";
                c1B.C1AllorsString = "1";
                c1C.C1AllorsString = "3";

                var extent = this.Transaction.Extent(m.C1);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                var sortedObjects = (C1[])extent.ToArray(typeof(C1));

                var names = sortedObjects.Select(v => v.C1AllorsString).ToArray();

                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(c1D, sortedObjects[0]);
                Assert.Equal(c1B, sortedObjects[1]);
                Assert.Equal(c1A, sortedObjects[2]);
                Assert.Equal(c1C, sortedObjects[3]);

                var c1AId = c1A.Id;

                this.Transaction.Commit();

                using (var transaction2 = this.CreateTransaction())
                {
                    c1A = (C1)transaction2.Instantiate(c1AId);

                    extent = transaction2.Extent(m.C1);
                    extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                    sortedObjects = (C1[])extent.ToArray(typeof(C1));

                    names = sortedObjects.Select(v => v.C1AllorsString).ToArray();

                    Assert.Equal(4, sortedObjects.Length);
                    Assert.Equal(c1D, sortedObjects[0]);
                    Assert.Equal(c1B, sortedObjects[1]);
                    Assert.Equal(c1A, sortedObjects[2]);
                    Assert.Equal(c1C, sortedObjects[3]);
                }
            }
        }
    }
}
