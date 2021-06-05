// <copyright file="SqlClientTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Adapters;
    using Domain;
    using Xunit;

    public class SqlClientTest : IDisposable, IClassFixture<Fixture<SqlClientTest>>
    {
        private readonly Profile profile;

        public SqlClientTest() => this.profile = new Profile(this.GetType().Name);

        protected IProfile Profile => this.profile;

        public void Dispose() => this.profile.Dispose();

        #region Population
#pragma warning disable IDE1006
        protected C1 c1A;
        protected C1 c1B;
        protected C1 c1C;
        protected C1 c1D;
        protected C2 c2A;
        protected C2 c2B;
        protected C2 c2C;
        protected C2 c2D;
        protected C3 c3A;
        protected C3 c3B;
        protected C3 c3C;
        protected C3 c3D;
        protected C4 c4A;
        protected C4 c4B;
        protected C4 c4C;
        protected C4 c4D;
#pragma warning restore IDE1006
        #endregion

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        [Fact]
        public void Bulk()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var count = Settings.LargeArraySize;

                using (var transaction = this.CreateTransaction())
                {
                    var c1s = (Extent<C1>)transaction.Create(m.C1, count);
                    var c2s = (Extent<C2>)transaction.Create(m.C2, count);

                    for (var i = 0; i < count; i++)
                    {
                        var c1 = c1s[i];

                        c1.C1C2many2one = c2s[i];

                        for (var j = 0; j < 10; j++)
                        {
                            var c2 = c2s[j];
                            c1.AddC1C2many2many(c2);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        [Fact]
        public void Prefetch()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c2PrefetchPolicy = new PrefetchPolicyBuilder()
                    .WithRule(m.C2.C3Many2Manies)
                    .Build();

                var c1PrefetchPolicy = new PrefetchPolicyBuilder()
                    .WithRule(m.C1.C1C2many2manies, c2PrefetchPolicy)
                    .Build();

                this.Populate();

                this.Transaction.Commit();

                var c1s = this.Transaction.Extent<C1>().ToArray();
                this.Transaction.Prefetch(c1PrefetchPolicy, c1s);

                foreach (C2 c2 in this.Transaction.Extent<C2>())
                {
                    c2.Strategy.Delete();

                    foreach (C3 c3 in this.Transaction.Extent<C3>())
                    {
                        c3.Strategy.Delete();

                        c1s = this.Transaction.Extent<C1>().ToArray();
                        this.Transaction.Prefetch(c1PrefetchPolicy, c1s);
                    }
                }
            }
        }

        protected void Populate()
        {
            var population = new TestPopulation(this.Transaction);

            this.c1A = population.C1A;
            this.c1B = population.C1B;
            this.c1C = population.C1C;
            this.c1D = population.C1D;

            this.c2A = population.C2A;
            this.c2B = population.C2B;
            this.c2C = population.C2C;
            this.c2D = population.C2D;

            this.c3A = population.C3A;
            this.c3B = population.C3B;
            this.c3C = population.C3C;
            this.c3D = population.C3D;

            this.c4A = population.C4A;
            this.c4B = population.C4B;
            this.c4C = population.C4C;
            this.c4D = population.C4D;
        }

        protected ITransaction CreateTransaction() => this.Profile.Database.CreateTransaction();
    }
}
