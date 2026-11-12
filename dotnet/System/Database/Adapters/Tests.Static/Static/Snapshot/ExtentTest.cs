// <copyright file="ExtentTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Linq;
    using Adapters;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;
    using C3 = Domain.C3;

    public class ExtentTest : Adapters.ExtentTest
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();

        [Fact]
        public void BinaryEqualsUsesValueEquality()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;
                var matching = C1.Create(this.Transaction);
                var different = C1.Create(this.Transaction);
                var empty = C1.Create(this.Transaction);
                var absent = C1.Create(this.Transaction);

                matching.C1AllorsBinary = new byte[] { 1, 2 };
                matching.I1AllorsBinary = new byte[] { 1, 2 };
                different.C1AllorsBinary = new byte[] { 1, 2, 3 };
                different.I1AllorsBinary = new byte[] { 9 };
                empty.C1AllorsBinary = Array.Empty<byte>();
                empty.I1AllorsBinary = Array.Empty<byte>();
                this.Transaction.Commit();

                var extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsBinary, new byte[] { 1, 2 });
                Assert.Equal(matching, Assert.Single(extent.Cast<C1>()));

                extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsBinary, new byte[] { 2, 1 });
                Assert.Empty(extent);

                extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsBinary, Array.Empty<byte>());
                Assert.Equal(empty, Assert.Single(extent.Cast<C1>()));

                extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsBinary, m.C1.I1AllorsBinary);
                Assert.Equal(2, extent.Count);
                Assert.Contains(matching, extent);
                Assert.Contains(empty, extent);

                matching.RemoveI1AllorsBinary();
                extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddNot().AddEquals(m.C1.C1AllorsBinary, m.C1.I1AllorsBinary);
                Assert.Equal(2, extent.Count);
                Assert.Contains(matching, extent);
                Assert.Contains(different, extent);
                Assert.DoesNotContain(absent, extent);
            }
        }

        // An extent finds the objects of its type among those that the transaction created. Going through
        // every created object for that takes time and memory in proportion to their number for each
        // extent: a copy of 32 KB of ids for an extent of one object beside 4,000 objects of another class.
        [Fact]
        public void AnExtentDoesNotGoThroughEveryObjectCreatedInTheTransaction()
        {
            const int Count = 4000;
            const int Extents = 100;
            const long Limit = Extents * 4 * 1024L;

            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = C1.Create(this.Transaction);
                C2.Create(this.Transaction, Count);

                var before = GC.GetAllocatedBytesForCurrentThread();

                for (var i = 0; i < Extents; i++)
                {
                    Assert.Equal(c1, Assert.Single(this.Transaction.Extent(m.C1).Cast<C1>()));
                }

                Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, Limit);
            }
        }

        // The objects that the transaction created follow the committed ones in the order of their
        // creation, whatever their class.
        [Fact]
        public void AnExtentHoldsTheObjectsCreatedInTheTransactionInTheOrderOfTheirCreation()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var committed = C2.Create(this.Transaction);
                this.Transaction.Commit();

                var first = C2.Create(this.Transaction);
                var second = C1.Create(this.Transaction);
                C3.Create(this.Transaction);
                var deleted = C1.Create(this.Transaction);
                var third = C2.Create(this.Transaction);
                var fourth = C1.Create(this.Transaction);
                deleted.Strategy.Delete();

                Assert.Equal(new IObject[] { committed, first, second, third, fourth }, this.Transaction.Extent(m.I12).Cast<IObject>().ToArray());
                Assert.Equal(new IObject[] { second, fourth }, this.Transaction.Extent(m.C1).Cast<IObject>().ToArray());
                Assert.Equal(new IObject[] { committed, first, third }, this.Transaction.Extent(m.C2).Cast<IObject>().ToArray());
            }
        }
    }
}
