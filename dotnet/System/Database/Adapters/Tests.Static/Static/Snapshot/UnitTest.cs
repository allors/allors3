// <copyright file="UnitTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the Default type.
// </summary>

namespace Allors.Database.Adapters.Snapshot
{
    using Adapters;
    using Xunit;
    using C1 = Domain.C1;

    public class UnitTest : Adapters.UnitTest
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        [Fact]
        public void BinaryValuesAreIsolatedFromCallerBuffers()
        {
            foreach (var init in this.profile.Inits)
            {
                init();
                var database = (Database)this.profile.Database;
                var transaction = this.profile.Transaction;
                var value = C1.Create(transaction);
                var input = new byte[] { 1 };
                value.C1AllorsBinary = input;
                input[0] = 2;
                Assert.Equal(new byte[] { 1 }, value.C1AllorsBinary);
                transaction.Commit();

                var version = value.Strategy.ObjectVersion;
                var restorePoint = database.TakeRestorePoint();
                var clone = database.CreateClone(new DefaultDomainDatabaseServices());
                using (var reader = database.CreateTransaction())
                using (var cloneTransaction = clone.CreateTransaction())
                {
                    var readValue = (C1)reader.Instantiate(value.Id);
                    var clonedValue = (C1)cloneTransaction.Instantiate(value.Id);
                    input[0] = 3;
                    value.C1AllorsBinary[0] = 4;
                    readValue.C1AllorsBinary[0] = 5;
                    clonedValue.C1AllorsBinary[0] = 6;

                    transaction.Rollback();
                    Assert.Equal(new byte[] { 1 }, value.C1AllorsBinary);
                    Assert.Equal(new byte[] { 1 }, readValue.C1AllorsBinary);
                    Assert.Equal(new byte[] { 1 }, clonedValue.C1AllorsBinary);
                    Assert.Equal(version, value.Strategy.ObjectVersion);

                    value.C1AllorsBinary = new byte[] { 7 };
                    transaction.Commit();
                    Assert.Equal(new byte[] { 1 }, readValue.C1AllorsBinary);
                    Assert.Equal(new byte[] { 1 }, clonedValue.C1AllorsBinary);
                }

                database.Restore(restorePoint);
                transaction.Rollback();
                Assert.Equal(new byte[] { 1 }, value.C1AllorsBinary);
                Assert.Equal(version, value.Strategy.ObjectVersion);

                value.C1AllorsBinary = value.C1AllorsBinary;
                Assert.Empty(transaction.Checkpoint().Associations);
                transaction.Commit();
                Assert.Equal(version, value.Strategy.ObjectVersion);

                value.C1AllorsBinary = new byte[] { 8 };
                value.C1AllorsBinary = new byte[] { 1 };
                Assert.Empty(transaction.Checkpoint().Associations);
            }
        }
    }
}
