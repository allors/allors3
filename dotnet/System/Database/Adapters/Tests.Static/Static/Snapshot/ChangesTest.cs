// <copyright file="ChangesTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using Adapters;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    public class ChangesTest : Adapters.ChangesTest, IDisposable
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        // The change log keeps the roles and associations from before the first change in a checkpoint. Reading
        // them again for every object that joins or leaves takes time and memory in proportion to the square of
        // their number: about 130 MiB for each of the loops below, where reading them once takes less than 9 MiB.
        [Fact]
        public void AChangeDoesNotReadTheWholeRoleOrAssociationAgain()
        {
            const int Count = 4000;
            const long Limit = Count * 8 * 1024L;

            foreach (var init in this.profile.Inits)
            {
                init();
                var transaction = this.profile.Transaction;

                var c1 = C1.Create(transaction);
                var c2s = C2.Create(transaction, Count);
                var c1s = C1.Create(transaction, Count);
                var c2 = C2.Create(transaction);
                transaction.Commit();

                Assert.InRange(Allocated(Count, i => c1.AddC1C2many2many(c2s[i])), 0, Limit);
                Assert.InRange(Allocated(Count, i => c1.RemoveC1C2many2many(c2s[i])), 0, Limit);

                Assert.InRange(Allocated(Count, i => c1.AddC1C2one2many(c2s[i])), 0, Limit);
                Assert.InRange(Allocated(Count, i => c1.RemoveC1C2one2many(c2s[i])), 0, Limit);

                Assert.InRange(Allocated(Count, i => c1s[i].C1C2many2one = c2), 0, Limit);
                Assert.InRange(Allocated(Count, i => c1s[i].RemoveC1C2many2one()), 0, Limit);
            }
        }

        private static long Allocated(int count, Action<int> change)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();

            for (var i = 0; i < count; i++)
            {
                change(i);
            }

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
    }
}
