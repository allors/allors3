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

        [Fact]
        public override void SortTwo()
        {
            base.SortTwo();

            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;
                foreach (var value in new[] { 3, 1, 2 })
                {
                    C1.Create(this.Transaction).C1AllorsInteger = value;
                }

                this.Transaction.Commit();

                foreach (var primaryDirection in new[] { SortDirection.Ascending, SortDirection.Descending })
                {
                    foreach (var secondaryDirection in new[] { SortDirection.Ascending, SortDirection.Descending })
                    {
                        var extent = this.Transaction.Extent(m.C1);
                        extent.AddSort(m.C1.C1AllorsString, primaryDirection);
                        extent.AddSort(m.C1.C1AllorsInteger, secondaryDirection);

                        var expected = secondaryDirection == SortDirection.Ascending ? new int?[] { 1, 2, 3 } : new int?[] { 3, 2, 1 };
                        Assert.Equal(expected, extent.Cast<C1>().Select(v => v.C1AllorsInteger));
                    }
                }
            }
        }
    }
}
