// <copyright file="FilteredExtentLocalChangesTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using Domain;
    using Xunit;

    /// <summary>
    /// Regression tests ensuring filtered extent queries correctly reflect
    /// transaction-local modifications (uncommitted changes).
    /// These tests would have caught bugs where query optimizations returned
    /// stale results from committed data instead of current transaction state.
    /// </summary>
    public abstract class FilteredExtentLocalChangesTest : IDisposable
    {
        protected abstract IProfile Profile { get; }

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void QueryAfterLocalModificationShouldReflectNewValue()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // Arrange: Create and commit an object with a specific string value
                var c1 = (C1)this.Transaction.Create(m.C1);
                c1.C1AllorsString = "OriginalValue";
                this.Transaction.Commit();

                // Act: Query for original value - should find object
                var extent1 = this.Transaction.Extent(m.C1);
                extent1.Filter.AddEquals(m.C1.C1AllorsString, "OriginalValue");
                Assert.Single(extent1);
                Assert.Contains(c1, extent1);

                // Act: Modify locally without commit
                c1.C1AllorsString = "ModifiedValue";

                // Assert: Query for OLD value should NOT find object anymore
                var extent2 = this.Transaction.Extent(m.C1);
                extent2.Filter.AddEquals(m.C1.C1AllorsString, "OriginalValue");
                Assert.Empty(extent2);

                // Assert: Query for NEW value should find object
                var extent3 = this.Transaction.Extent(m.C1);
                extent3.Filter.AddEquals(m.C1.C1AllorsString, "ModifiedValue");
                Assert.Single(extent3);
                Assert.Contains(c1, extent3);
            }
        }

        [Fact]
        public void QueryAfterLocalModificationToNullShouldNotFindObject()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = (C1)this.Transaction.Create(m.C1);
                c1.C1AllorsString = "SomeValue";
                this.Transaction.Commit();

                // Act: Modify to null locally
                c1.C1AllorsString = null;

                // Assert: Query for original value should not find object
                var extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsString, "SomeValue");
                Assert.Empty(extent);
            }
        }

        [Fact]
        public void QueryAfterLocalModificationFromNullShouldFindObject()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = (C1)this.Transaction.Create(m.C1);
                c1.C1AllorsString = null;
                this.Transaction.Commit();

                // Act: Modify from null to value locally
                c1.C1AllorsString = "NewValue";

                // Assert: Query for new value should find object
                var extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsString, "NewValue");
                Assert.Single(extent);
                Assert.Contains(c1, extent);
            }
        }

        [Fact]
        public void QueryAfterNewObjectCreationShouldFindObject()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // Arrange: Start with committed data
                var existing = (C1)this.Transaction.Create(m.C1);
                existing.C1AllorsString = "Existing";
                this.Transaction.Commit();

                // Act: Create NEW object (not committed yet)
                var newObj = (C1)this.Transaction.Create(m.C1);
                newObj.C1AllorsString = "BrandNew";

                // Assert: Query should find the new uncommitted object
                var extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsString, "BrandNew");
                Assert.Single(extent);
                Assert.Contains(newObj, extent);
            }
        }

        [Fact]
        public void QueryAfterDeleteShouldNotFindObject()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = (C1)this.Transaction.Create(m.C1);
                c1.C1AllorsString = "ToDelete";
                this.Transaction.Commit();

                // Act: Delete locally
                c1.Strategy.Delete();

                // Assert: Query should not find deleted object
                var extent = this.Transaction.Extent(m.C1);
                extent.Filter.AddEquals(m.C1.C1AllorsString, "ToDelete");
                Assert.Empty(extent);
            }
        }

        [Fact]
        public void RollbackShouldRestoreQueryResults()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = (C1)this.Transaction.Create(m.C1);
                c1.C1AllorsString = "Original";
                this.Transaction.Commit();

                // Act: Modify and rollback
                c1.C1AllorsString = "Modified";

                var extentBeforeRollback = this.Transaction.Extent(m.C1);
                extentBeforeRollback.Filter.AddEquals(m.C1.C1AllorsString, "Original");
                Assert.Empty(extentBeforeRollback);

                this.Transaction.Rollback();

                // Assert: After rollback, original query should work
                var extentAfterRollback = this.Transaction.Extent(m.C1);
                extentAfterRollback.Filter.AddEquals(m.C1.C1AllorsString, "Original");
                Assert.Single(extentAfterRollback);
            }
        }
    }
}
