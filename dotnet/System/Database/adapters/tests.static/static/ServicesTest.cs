// <copyright file="ServicesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;
    using ClassWithoutRoles = Domain.ClassWithoutRoles;
    using ClassWithoutUnitRoles = Domain.ClassWithoutUnitRoles;

    public abstract class ServicesTest : IDisposable
    {
        protected static readonly bool[] TrueFalse = { true, false };

        protected abstract IProfile Profile { get; }

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void NextId()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = this.Transaction.Create<C1>();
                var strategy = c1.Strategy;
                var id = long.Parse(strategy.ObjectId.ToString());

                var nextId = long.Parse(this.Transaction.Create<C1>().Strategy.ObjectId.ToString());

                Assert.Equal(id + 1, nextId);
            }
        }

        [Fact]
        public void CreateMany()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                int[] runs = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };

                var total = 0;
                foreach (var run in runs)
                {
                    var allorsObjects = this.Transaction.Create(m.C1, run);

                    Assert.Equal(run, allorsObjects.Length);

                    total += run;

                    Assert.Equal(total, this.GetExtent(m.C1).Length);

                    var ids = new ArrayList();
                    foreach (C1 allorsObject in allorsObjects)
                    {
                        Assert.Equal(m.C1, allorsObject.Strategy.Class);
                        ids.Add(allorsObject.Strategy.ObjectId);
                        allorsObject.C1AllorsString = "CreateMany";
                    }

                    Assert.Equal(run, ids.Count);

                    this.Transaction.Commit();

                    allorsObjects = this.Transaction.Instantiate((long[])ids.ToArray(typeof(long)));
                    foreach (C1 allorsObject in allorsObjects)
                    {
                        Assert.Equal(m.C1, allorsObject.Strategy.Class);
                        allorsObject.C1AllorsString = "CreateMany";
                    }

                    var c2s = (C2[])this.Transaction.Create(m.C2, run);
                    Assert.Equal(run, c2s.Length);
                }
            }
        }

        [Fact]
        public void UnitRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);

                this.Transaction.Commit();

                c1A.C1AllorsString = "1";

                this.Transaction.Commit();

                c1A.C1AllorsString = "2";

                this.Transaction.Rollback();

                Assert.Equal("1", c1A.C1AllorsString);
            }
        }

        [Fact]
        public void CompositeRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);

                var c2A = C2.Create(this.Transaction);
                var c2B = C2.Create(this.Transaction);

                this.Transaction.Commit();

                c1A.C1C2one2one = c2A;

                this.Transaction.Commit();

                c1A.C1C2one2one = c2B;

                this.Transaction.Rollback();

                Assert.Equal(c2A, c1A.C1C2one2one);
            }
        }

        [Fact]
        public void CompositeRoles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);

                var c2A = C2.Create(this.Transaction);
                var c2B = C2.Create(this.Transaction);

                this.Transaction.Commit();

                c1A.AddC1C2one2many(c2A);

                this.Transaction.Commit();

                c1A.AddC1C2one2many(c2B);

                this.Transaction.Rollback();

                Assert.Single(c1A.C1C2one2manies);
                Assert.Equal(c2A, c1A.C1C2one2manies.ElementAt(0));
            }
        }

        [Fact]
        public void Delete()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // Object
                var anObject = C1.Create(this.Transaction);

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);
                anObject = (C1)this.Transaction.Instantiate(anObject.Strategy.ObjectId);
                Assert.Null(anObject);

                //// Commit & Rollback

                anObject = C1.Create(this.Transaction);
                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                var id = anObject.Strategy.ObjectId;
                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Commit + Commit + Commit

                anObject = C1.Create(this.Transaction);

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Nothing + Commit + Rollback

                anObject = C1.Create(this.Transaction);

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Commit + Commit + Rollback

                anObject = C1.Create(this.Transaction);

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Nothing + Rollback + Rollback

                anObject = C1.Create(this.Transaction);
                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Commit + Rollback + Rollback

                anObject = C1.Create(this.Transaction);

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.False(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(anObject.Strategy.IsDeleted);

                //// Nothing + Rollback + Commit

                anObject = C1.Create(this.Transaction);

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.Null(anObject);

                //// Commit + Rollback + Commit

                anObject = C1.Create(this.Transaction);

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.False(anObject.Strategy.IsDeleted);

                // instantiate
                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;

                this.Transaction.Commit();

                anObject.Strategy.Delete();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(anObject.Strategy.IsDeleted);

                // Clean up
                this.Transaction.Commit();
                foreach (C1 removeObject in this.GetExtent(m.C1))
                {
                    removeObject.Strategy.Delete();
                }

                this.Transaction.Commit();

                // Strategy
                if (this.Transaction is ITransaction databaseTransaction)
                {
                    var aStrategy = C1.Create(this.Transaction).Strategy;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);
                    aStrategy = databaseTransaction.InstantiateStrategy(aStrategy.ObjectId);
                    Assert.Null(aStrategy);

                    //// Commit & Rollback

                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;
                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Commit + Commit + Commit

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Nothing + Commit + Rollback

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Commit + Commit + Rollback

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Nothing + Rollback + Rollback

                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Commit + Rollback + Rollback

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.False(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.False(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.False(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.False(aStrategy.IsDeleted);

                    //// Nothing + Rollback + Commit

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.True(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.Null(aStrategy);

                    //// Commit + Rollback + Commit

                    aStrategy = C1.Create(databaseTransaction).Strategy;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    Assert.False(aStrategy.IsDeleted);

                    databaseTransaction.Commit();
                    Assert.False(aStrategy.IsDeleted);

                    // instantiate
                    aStrategy = C1.Create(databaseTransaction).Strategy;
                    id = aStrategy.ObjectId;

                    databaseTransaction.Commit();

                    aStrategy.Delete();
                    Assert.True(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.False(aStrategy.IsDeleted);

                    databaseTransaction.Rollback();
                    aStrategy = databaseTransaction.InstantiateStrategy(id);
                    Assert.False(aStrategy.IsDeleted);

                    // Clean up
                    databaseTransaction.Commit();
                    foreach (C1 removeObject in this.GetExtent(m.C1))
                    {
                        removeObject.Strategy.Delete();
                    }

                    databaseTransaction.Commit();
                }

                //// Units

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                anObject.Strategy.Delete();

                StrategyAssert.RoleExistHasException(anObject, m.C1.C1AllorsString);

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                anObject.Strategy.Delete();

                StrategyAssert.RoleGetHasException(anObject, m.C1.C1AllorsString);

                var secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                var thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(2, this.GetExtent(m.C1).Length);
                thirdObject.Strategy.Delete();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("b", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject.Strategy.Delete();

                //// Cached

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";

                AllorsTestUtils.ForceRoleCaching(anObject);

                anObject.Strategy.Delete();

                StrategyAssert.RoleExistHasException(anObject, m.C1.C1AllorsString);

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";

                AllorsTestUtils.ForceRoleCaching(anObject);

                anObject.Strategy.Delete();

                StrategyAssert.RoleGetHasException(anObject, m.C1.C1AllorsString);

                secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(2, this.GetExtent(m.C1).Length);

                AllorsTestUtils.ForceRoleCaching(secondObject);
                AllorsTestUtils.ForceRoleCaching(thirdObject);

                thirdObject.Strategy.Delete();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("b", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject.Strategy.Delete();

                //// Commit

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                anObject.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleExistHasException(anObject, m.C1.C1AllorsString);

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                anObject.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(anObject, m.C1.C1AllorsString);

                secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(2, this.GetExtent(m.C1).Length);
                thirdObject.Strategy.Delete();

                this.Transaction.Commit();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("b", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject.Strategy.Delete();

                this.Transaction.Commit();

                //// Cached

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";

                AllorsTestUtils.ForceRoleCaching(anObject);

                anObject.Strategy.Delete();
                this.Transaction.Commit();

                StrategyAssert.RoleExistHasException(anObject, m.C1.C1AllorsString);

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";

                AllorsTestUtils.ForceRoleCaching(anObject);

                anObject.Strategy.Delete();
                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(anObject, m.C1.C1AllorsString);

                secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(2, this.GetExtent(m.C1).Length);

                AllorsTestUtils.ForceRoleCaching(secondObject);
                AllorsTestUtils.ForceRoleCaching(thirdObject);

                thirdObject.Strategy.Delete();
                this.Transaction.Commit();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("b", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject.Strategy.Delete();
                this.Transaction.Commit();

                //// Rollback

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                this.Transaction.Commit();

                anObject.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("a", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(3, this.GetExtent(m.C1).Length);
                thirdObject.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("a", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                anObject.Strategy.Delete();

                this.Transaction.Commit();

                //// Cached

                anObject = C1.Create(this.Transaction);
                anObject.C1AllorsString = "a";
                this.Transaction.Commit();

                AllorsTestUtils.ForceRoleCaching(anObject);

                anObject.Strategy.Delete();
                this.Transaction.Rollback();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("a", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                secondObject = C1.Create(this.Transaction);
                secondObject.C1AllorsString = "b";
                thirdObject = C1.Create(this.Transaction);
                thirdObject.C1AllorsString = "c";

                Assert.Equal(3, this.GetExtent(m.C1).Length);

                AllorsTestUtils.ForceRoleCaching(secondObject);
                AllorsTestUtils.ForceRoleCaching(thirdObject);

                thirdObject.Strategy.Delete();
                this.Transaction.Rollback();

                Assert.Single(this.GetExtent(m.C1));
                Assert.Equal("a", ((C1)this.GetExtent(m.C1)[0]).C1AllorsString);

                anObject.Strategy.Delete();
                this.Transaction.Commit();

                //// IComposite

                //// Role

                var fromC1a = C1.Create(this.Transaction);
                var fromC1b = C1.Create(this.Transaction);
                var fromC1c = C1.Create(this.Transaction);
                var fromC1d = C1.Create(this.Transaction);

                var toC1a = C1.Create(this.Transaction);
                var toC1b = C1.Create(this.Transaction);
                var toC1c = C1.Create(this.Transaction);
                var toC1d = C1.Create(this.Transaction);

                var toC2a = C2.Create(this.Transaction);
                var toC2b = C2.Create(this.Transaction);
                var toC2c = C2.Create(this.Transaction);
                var toC2d = C2.Create(this.Transaction);

                //// C1 <-> C1

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C1one2one);
                Assert.Empty(fromC1a.C1C1one2manies);
                Assert.Null(fromC1a.C1C1many2one);
                Assert.Empty(fromC1a.C1C1many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);
                Assert.Equal(toC1c, fromC1b.C1C1one2one);
                Assert.Equal(2, fromC1b.C1C1one2manies.Count());
                Assert.Equal(toC1c, fromC1b.C1C1many2one);
                Assert.Equal(2, fromC1b.C1C1many2manies.Count());

                this.Transaction.Commit();

                //// C1 <-> C2

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationExistHasException(toC1a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC1a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationExistHasException(toC1b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC1b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationGetHasException(toC1b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC1b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C2.C1sWhereC1C2many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C2one2one);
                Assert.Empty(fromC1a.C1C2one2manies);
                Assert.Null(fromC1a.C1C2many2one);
                Assert.Empty(fromC1a.C1C2many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);
                Assert.Equal(toC2c, fromC1b.C1C2one2one);
                Assert.Equal(2, fromC1b.C1C2one2manies.Count());
                Assert.Equal(toC2c, fromC1b.C1C2many2one);
                Assert.Equal(2, fromC1b.C1C2many2manies.Count());

                this.Transaction.Commit();

                //// Commit

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C1one2one);
                Assert.Empty(fromC1a.C1C1one2manies);
                Assert.Null(fromC1a.C1C1many2one);
                Assert.Empty(fromC1a.C1C1many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);
                Assert.Equal(toC1c, fromC1b.C1C1one2one);
                Assert.Equal(2, fromC1b.C1C1one2manies.Count());
                Assert.Equal(toC1c, fromC1b.C1C1many2one);
                Assert.Equal(2, fromC1b.C1C1many2manies.Count());

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1sWhereC1C2many2many);

                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1sWhereC1C2many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C2one2one);
                Assert.Empty(fromC1a.C1C2one2manies);
                Assert.Null(fromC1a.C1C2many2one);
                Assert.Empty(fromC1a.C1C2many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);
                Assert.Equal(toC2c, fromC1b.C1C2one2one);
                Assert.Equal(2, fromC1b.C1C2one2manies.Count());
                Assert.Equal(toC2c, fromC1b.C1C2many2one);
                Assert.Equal(2, fromC1b.C1C2many2manies.Count());

                //// Rollback

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);

                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.True(toC1c.Strategy.IsDeleted);
                Assert.True(toC1d.Strategy.IsDeleted);

                // Commit + Rollback
                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                this.Transaction.Commit();

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);

                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC1a.C1WhereC1C1one2one);
                Assert.Equal(fromC1a, toC1b.C1WhereC1C1one2many);
                Assert.Single(toC1a.C1sWhereC1C1many2one);
                Assert.Single(toC1a.C1sWhereC1C1many2many);
                Assert.Single(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);

                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.True(toC2c.Strategy.IsDeleted);
                Assert.True(toC2d.Strategy.IsDeleted);

                // Commit + Rollback
                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                this.Transaction.Commit();

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);

                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC2a.C1WhereC1C2one2one);
                Assert.Equal(fromC1a, toC2b.C1WhereC1C2one2many);
                Assert.Single(toC2a.C1sWhereC1C2many2one);
                Assert.Single(toC2a.C1sWhereC1C2many2many);
                Assert.Single(toC2b.C1sWhereC1C2many2many);

                //// Cached

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C1one2one);
                Assert.Empty(fromC1a.C1C1one2manies);
                Assert.Null(fromC1a.C1C1many2one);
                Assert.Empty(fromC1a.C1C1many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);
                Assert.Equal(toC1c, fromC1b.C1C1one2one);
                Assert.Equal(2, fromC1b.C1C1one2manies.Count());
                Assert.Equal(toC1c, fromC1b.C1C1many2one);
                Assert.Equal(2, fromC1b.C1C1many2manies.Count());

                this.Transaction.Commit();

                //// Commit

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1a, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationExistHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1WhereC1C1one2many);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2one);
                StrategyAssert.AssociationGetHasException(toC1b, m.C1.C1sWhereC1C1many2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C1one2one);
                Assert.Empty(fromC1a.C1C1one2manies);
                Assert.Null(fromC1a.C1C1many2one);
                Assert.Empty(fromC1a.C1C1many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);
                Assert.Equal(toC1c, fromC1b.C1C1one2one);
                Assert.Equal(2, fromC1b.C1C1one2manies.Count());
                Assert.Equal(toC1c, fromC1b.C1C1many2one);
                Assert.Equal(2, fromC1b.C1C1many2manies.Count());

                this.Transaction.Commit();

                //// Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);

                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.True(toC1c.Strategy.IsDeleted);
                Assert.True(toC1d.Strategy.IsDeleted);

                //// Commit + Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1b.C1C1one2one = toC1c;

                fromC1a.C1C1many2one = toC1a;
                fromC1b.C1C1many2one = toC1c;

                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);

                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                this.Transaction.Commit();

                toC1a.Strategy.Delete();
                toC1b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);

                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC1a.C1WhereC1C1one2one);
                Assert.Equal(fromC1a, toC1b.C1WhereC1C1one2many);
                Assert.Single(toC1a.C1sWhereC1C1many2one);
                Assert.Single(toC1a.C1sWhereC1C1many2many);
                Assert.Single(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C2one2one);
                Assert.Empty(fromC1a.C1C2one2manies);
                Assert.Null(fromC1a.C1C2many2one);
                Assert.Empty(fromC1a.C1C2many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);
                Assert.Equal(toC2c, fromC1b.C1C2one2one);
                Assert.Equal(2, fromC1b.C1C2one2manies.Count());
                Assert.Equal(toC2c, fromC1b.C1C2many2one);
                Assert.Equal(2, fromC1b.C1C2many2manies.Count());

                this.Transaction.Commit();

                //// Commit

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2a, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2a, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationExistHasException(toC2b, m.C2.C1WhereC1C2one2many);

                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2many);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1sWhereC1C2many2one);
                StrategyAssert.AssociationGetHasException(toC2b, m.C2.C1WhereC1C2one2many);

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.Null(fromC1a.C1C2one2one);
                Assert.Empty(fromC1a.C1C2one2manies);
                Assert.Null(fromC1a.C1C2many2one);
                Assert.Empty(fromC1a.C1C2many2manies);

                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);
                Assert.Equal(toC2c, fromC1b.C1C2one2one);
                Assert.Equal(2, fromC1b.C1C2one2manies.Count());
                Assert.Equal(toC2c, fromC1b.C1C2many2one);
                Assert.Equal(2, fromC1b.C1C2many2manies.Count());

                this.Transaction.Commit();

                //// Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);

                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.True(toC2c.Strategy.IsDeleted);
                Assert.True(toC2d.Strategy.IsDeleted);

                // Commit + Rollback
                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1b.C1C2one2one = toC2c;

                fromC1a.C1C2many2one = toC2a;
                fromC1b.C1C2many2one = toC2c;

                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);

                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                this.Transaction.Commit();

                toC2a.Strategy.Delete();
                toC2b.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);

                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC2a.C1WhereC1C2one2one);
                Assert.Equal(fromC1a, toC2b.C1WhereC1C2one2many);
                Assert.Single(toC2a.C1sWhereC1C2many2one);
                Assert.Single(toC2a.C1sWhereC1C2many2many);
                Assert.Single(toC2b.C1sWhereC1C2many2many);

                //// Association

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                fromC1a.Strategy.Delete();
                fromC1b.Strategy.Delete();

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.Null(toC1a.C1WhereC1C1one2one);
                Assert.Null(toC1b.C1WhereC1C1one2many);
                Assert.Empty(toC1a.C1sWhereC1C1many2one);
                Assert.Empty(toC1a.C1sWhereC1C1many2many);
                Assert.Empty(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                fromC1a.Strategy.Delete();
                fromC1b.Strategy.Delete();

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2manies);

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.Null(toC2a.C1WhereC1C2one2one);
                Assert.Null(toC2b.C1WhereC1C2one2many);
                Assert.Empty(toC2a.C1sWhereC1C2many2one);
                Assert.Empty(toC2a.C1sWhereC1C2many2many);
                Assert.Empty(toC2b.C1sWhereC1C2many2many);

                //// Commit

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                fromC1a.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.Null(toC1a.C1WhereC1C1one2one);
                Assert.Null(toC1b.C1WhereC1C1one2many);
                Assert.Empty(toC1a.C1sWhereC1C1many2one);
                Assert.Empty(toC1a.C1sWhereC1C1many2many);
                Assert.Empty(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                fromC1a.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.Null(toC2a.C1WhereC1C2one2one);
                Assert.Null(toC2b.C1WhereC1C2one2many);
                Assert.Empty(toC2a.C1sWhereC1C2many2one);
                Assert.Empty(toC2a.C1sWhereC1C2many2many);
                Assert.Empty(toC2b.C1sWhereC1C2many2many);

                //// Rollback

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1many2manies);

                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.True(toC1c.Strategy.IsDeleted);
                Assert.True(toC1d.Strategy.IsDeleted);

                // Commit + Rollback
                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                this.Transaction.Commit();

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC1a.C1WhereC1C1one2one);
                Assert.Equal(fromC1a, toC1b.C1WhereC1C1one2many);
                Assert.Single(toC1a.C1sWhereC1C1many2one);
                Assert.Single(toC1a.C1sWhereC1C1many2many);
                Assert.Single(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2manies);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2manies);

                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2one2manies);

                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2one2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.True(toC2c.Strategy.IsDeleted);
                Assert.True(toC2d.Strategy.IsDeleted);

                // Commit + Rollback
                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                this.Transaction.Commit();

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC2a.C1WhereC1C2one2one);
                Assert.Equal(fromC1a, toC2b.C1WhereC1C2one2many);
                Assert.Single(toC2a.C1sWhereC1C2many2one);
                Assert.Single(toC2a.C1sWhereC1C2many2many);
                Assert.Single(toC2b.C1sWhereC1C2many2many);

                //// Cached

                //// C1 <-> C1

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceAssociationCaching(toC1a);
                AllorsTestUtils.ForceAssociationCaching(toC1a);

                fromC1a.Strategy.Delete();
                fromC1b.Strategy.Delete();

                Assert.Empty(toC1a.C1sWhereC1C1many2one);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.Null(toC1a.C1WhereC1C1one2one);
                Assert.Null(toC1b.C1WhereC1C1one2many);
                Assert.Empty(toC1a.C1sWhereC1C1many2one);
                Assert.Empty(toC1a.C1sWhereC1C1many2many);
                Assert.Empty(toC1b.C1sWhereC1C1many2many);

                //// Commit

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                fromC1a.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.Null(toC1a.C1WhereC1C1one2one);
                Assert.Null(toC1b.C1WhereC1C1one2many);
                Assert.Empty(toC1a.C1sWhereC1C1many2one);
                Assert.Empty(toC1a.C1sWhereC1C1many2many);
                Assert.Empty(toC1b.C1sWhereC1C1many2many);

                //// Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C1many2manies);

                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1one2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1one2manies);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1many2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C1many2manies);

                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1one2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1one2manies);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1many2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C1many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);
                Assert.True(toC1a.Strategy.IsDeleted);
                Assert.True(toC1b.Strategy.IsDeleted);
                Assert.True(toC1c.Strategy.IsDeleted);
                Assert.True(toC1d.Strategy.IsDeleted);

                //// Commit + Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC1a = C1.Create(this.Transaction);
                toC1b = C1.Create(this.Transaction);
                toC1c = C1.Create(this.Transaction);
                toC1d = C1.Create(this.Transaction);

                fromC1a.C1C1one2one = toC1a;
                fromC1a.AddC1C1one2many(toC1a);
                fromC1a.AddC1C1one2many(toC1b);
                fromC1a.C1C1many2one = toC1a;
                fromC1a.AddC1C1many2many(toC1a);
                fromC1a.AddC1C1many2many(toC1b);

                fromC1b.C1C1one2one = toC1b;
                fromC1b.AddC1C1one2many(toC1c);
                fromC1b.AddC1C1one2many(toC1d);
                fromC1b.C1C1many2one = toC1b;
                fromC1b.AddC1C1many2many(toC1c);
                fromC1b.AddC1C1many2many(toC1d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                this.Transaction.Commit();

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);
                Assert.False(toC1a.Strategy.IsDeleted);
                Assert.False(toC1b.Strategy.IsDeleted);
                Assert.False(toC1c.Strategy.IsDeleted);
                Assert.False(toC1d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC1a.C1WhereC1C1one2one);
                Assert.Equal(fromC1a, toC1b.C1WhereC1C1one2many);
                Assert.Single(toC1a.C1sWhereC1C1many2one);
                Assert.Single(toC1a.C1sWhereC1C1many2many);
                Assert.Single(toC1b.C1sWhereC1C1many2many);

                //// C1 <-> C2

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceAssociationCaching(toC2a);
                AllorsTestUtils.ForceAssociationCaching(toC2a);

                fromC1a.Strategy.Delete();
                fromC1b.Strategy.Delete();

                Assert.Empty(toC2a.C1sWhereC1C2many2one);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2manies);

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.Null(toC2a.C1WhereC1C2one2one);
                Assert.Null(toC2b.C1WhereC1C2one2many);
                Assert.Empty(toC2a.C1sWhereC1C2many2one);
                Assert.Empty(toC2a.C1sWhereC1C2many2many);
                Assert.Empty(toC2b.C1sWhereC1C2many2many);

                //// Commit

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                fromC1a.Strategy.Delete();

                this.Transaction.Commit();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.Null(toC2a.C1WhereC1C2one2one);
                Assert.Null(toC2b.C1WhereC1C2one2many);
                Assert.Empty(toC2a.C1sWhereC1C2many2one);
                Assert.Empty(toC2a.C1sWhereC1C2many2many);
                Assert.Empty(toC2b.C1sWhereC1C2many2many);

                //// Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1a, m.C1.C1C2many2manies);

                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1a, m.C1.C1C2many2manies);

                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2one2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2one2manies);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2many2one);
                StrategyAssert.RoleExistHasException(fromC1b, m.C1.C1C2many2manies);

                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2one2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2one2manies);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2many2one);
                StrategyAssert.RoleGetHasException(fromC1b, m.C1.C1C2many2manies);

                Assert.True(fromC1a.Strategy.IsDeleted);
                Assert.True(fromC1b.Strategy.IsDeleted);
                Assert.True(fromC1c.Strategy.IsDeleted);
                Assert.True(fromC1d.Strategy.IsDeleted);
                Assert.True(toC2a.Strategy.IsDeleted);
                Assert.True(toC2b.Strategy.IsDeleted);
                Assert.True(toC2c.Strategy.IsDeleted);
                Assert.True(toC2d.Strategy.IsDeleted);

                //// Commit + Rollback

                fromC1a = C1.Create(this.Transaction);
                fromC1b = C1.Create(this.Transaction);
                fromC1c = C1.Create(this.Transaction);
                fromC1d = C1.Create(this.Transaction);

                toC2a = C2.Create(this.Transaction);
                toC2b = C2.Create(this.Transaction);
                toC2c = C2.Create(this.Transaction);
                toC2d = C2.Create(this.Transaction);

                fromC1a.C1C2one2one = toC2a;
                fromC1a.AddC1C2one2many(toC2a);
                fromC1a.AddC1C2one2many(toC2b);
                fromC1a.C1C2many2one = toC2a;
                fromC1a.AddC1C2many2many(toC2a);
                fromC1a.AddC1C2many2many(toC2b);

                fromC1b.C1C2one2one = toC2b;
                fromC1b.AddC1C2one2many(toC2c);
                fromC1b.AddC1C2one2many(toC2d);
                fromC1b.C1C2many2one = toC2b;
                fromC1b.AddC1C2many2many(toC2c);
                fromC1b.AddC1C2many2many(toC2d);

                AllorsTestUtils.ForceRoleCaching(fromC1a);
                AllorsTestUtils.ForceRoleCaching(fromC1b);

                this.Transaction.Commit();

                fromC1a.Strategy.Delete();

                this.Transaction.Rollback();

                Assert.False(fromC1a.Strategy.IsDeleted);
                Assert.False(fromC1b.Strategy.IsDeleted);
                Assert.False(fromC1c.Strategy.IsDeleted);
                Assert.False(fromC1d.Strategy.IsDeleted);
                Assert.False(toC2a.Strategy.IsDeleted);
                Assert.False(toC2b.Strategy.IsDeleted);
                Assert.False(toC2c.Strategy.IsDeleted);
                Assert.False(toC2d.Strategy.IsDeleted);

                Assert.Equal(fromC1a, toC2a.C1WhereC1C2one2one);
                Assert.Equal(fromC1a, toC2b.C1WhereC1C2one2many);
                Assert.Single(toC2a.C1sWhereC1C2many2one);
                Assert.Single(toC2a.C1sWhereC1C2many2many);
                Assert.Single(toC2b.C1sWhereC1C2many2many);

                //// Assignment

                anObject = C1.Create(this.Transaction);
                var c1Removed = C1.Create(this.Transaction);
                c1Removed.Strategy.Delete();
                C1[] c1RemovedArray = { c1Removed };

                var error = false;
                try
                {
                    anObject.C1C1one2one = c1Removed;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.AddC1C1one2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1one2manies);

                error = false;
                try
                {
                    anObject.C1C1one2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1one2manies);

                error = false;
                try
                {
                    anObject.AddC1C1many2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1many2manies);

                error = false;
                try
                {
                    anObject.C1C1many2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1many2manies);

                //// Commit

                anObject = C1.Create(this.Transaction);
                c1Removed = C1.Create(this.Transaction);
                c1Removed.Strategy.Delete();
                c1RemovedArray = new C1[1];
                c1RemovedArray[0] = c1Removed;

                this.Transaction.Commit();

                error = false;
                try
                {
                    anObject.C1C1one2one = c1Removed;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.AddC1C1one2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1one2manies);

                error = false;
                try
                {
                    anObject.C1C1one2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1one2manies);

                error = false;
                try
                {
                    anObject.AddC1C1many2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1many2manies);

                error = false;
                try
                {
                    anObject.C1C1many2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);
                Assert.Empty(anObject.C1C1many2manies);

                //// Rollback

                anObject = C1.Create(this.Transaction);
                c1Removed = C1.Create(this.Transaction);
                c1RemovedArray = new C1[1];
                c1RemovedArray[0] = c1Removed;

                c1Removed.Strategy.Delete();

                this.Transaction.Rollback();

                error = false;
                try
                {
                    anObject.C1C1one2one = c1Removed;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.AddC1C1one2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.C1C1one2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.AddC1C1many2many(c1Removed);
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                error = false;
                try
                {
                    anObject.C1C1many2manies = c1RemovedArray;
                }
                catch
                {
                    error = true;
                }

                Assert.True(error);

                // Commit + Rollback
                anObject = C1.Create(this.Transaction);
                c1Removed = C1.Create(this.Transaction);
                c1RemovedArray = new C1[1];
                c1RemovedArray[0] = c1Removed;

                this.Transaction.Commit();

                c1Removed.Strategy.Delete();

                this.Transaction.Rollback();

                anObject.C1C1one2one = c1Removed;
                Assert.Equal(c1Removed, anObject.C1C1one2one);

                anObject.AddC1C1one2many(c1Removed);
                Assert.Single(anObject.C1C1one2manies);

                anObject.C1C1one2manies = c1RemovedArray;
                Assert.Single(anObject.C1C1one2manies);

                anObject.AddC1C1many2many(c1Removed);
                Assert.Single(anObject.C1C1many2manies);

                anObject.C1C1many2manies = c1RemovedArray;
                Assert.Single(anObject.C1C1many2manies);

                //// Proxy

                var proxy = C1.Create(this.Transaction);
                id = proxy.Strategy.ObjectId;
                this.Transaction.Commit();

                var subject = C1.Instantiate(this.Transaction, id);
                subject.Strategy.Delete();
                StrategyAssert.RoleExistHasException(proxy, m.C1.C1AllorsString);

                this.Transaction.Commit();

                proxy = C1.Create(this.Transaction);
                id = proxy.Strategy.ObjectId;
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                subject.Strategy.Delete();
                StrategyAssert.RoleGetHasException(proxy, m.C1.C1AllorsString);

                this.Transaction.Commit();

                //// Commit

                proxy = C1.Create(this.Transaction);
                id = proxy.Strategy.ObjectId;
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                subject.Strategy.Delete();
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                StrategyAssert.RoleExistHasException(proxy, m.C1.C1AllorsString);

                this.Transaction.Commit();

                proxy = C1.Create(this.Transaction);
                id = proxy.Strategy.ObjectId;
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                subject.Strategy.Delete();
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                StrategyAssert.RoleGetHasException(proxy, m.C1.C1AllorsString);

                this.Transaction.Commit();

                //// Rollback

                proxy = C1.Create(this.Transaction);
                id = proxy.Strategy.ObjectId;
                this.Transaction.Commit();

                subject = C1.Instantiate(this.Transaction, id);
                subject.Strategy.Delete();
                this.Transaction.Rollback();

                subject = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();

                //// unit roles

                anObject = C1.Create(this.Transaction);
                var anotherObject = C1.Create(this.Transaction);
                anotherObject.C1AllorsString = "value";
                anObject.Strategy.Delete();
                Assert.Equal("value", anotherObject.C1AllorsString);

                this.Transaction.Commit();

                Assert.Equal("value", anotherObject.C1AllorsString);

                anObject = C1.Create(this.Transaction);
                anotherObject = C1.Create(this.Transaction);
                anotherObject.C1AllorsString = "value";
                anObject.Strategy.Delete();

                this.Transaction.Commit();

                Assert.Equal("value", anotherObject.C1AllorsString);
            }
        }

        [Fact]
        public virtual void DifferentTransactions()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var secondTransaction = this.CreateTransaction();

                try
                {
                    var c1a = C1.Create(this.Transaction);
                    var c1b = C1.Create(this.Transaction);

                    var c2a = C2.Create(secondTransaction);
                    var c2b = C2.Create(secondTransaction);
                    C2[] c2Array = { c2a, c2b };

                    this.Transaction.Commit();
                    secondTransaction.Commit();

                    var exceptionThrown = false;
                    try
                    {
                        c1a.C1C2one2one = c2a;
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    exceptionThrown = false;
                    try
                    {
                        c1a.C1C2many2one = c2a;
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    exceptionThrown = false;
                    try
                    {
                        c1a.AddC1C2one2many(c2a);
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    exceptionThrown = false;
                    try
                    {
                        c1a.C1C2one2manies = c2Array;
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    exceptionThrown = false;
                    try
                    {
                        c1a.AddC1C2many2many(c2a);
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    exceptionThrown = false;
                    try
                    {
                        c1a.C1C2many2manies = c2Array;
                    }
                    catch
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);
                }
                finally
                {
                    secondTransaction.Commit();
                }
            }
        }

        [Fact]
        public void Identity()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var anObject = C1.Create(this.Transaction);
                var id = anObject.Strategy.ObjectId;
                var proxy = C1.Instantiate(this.Transaction, id);

                var anotherObject = C1.Create(this.Transaction);
                var anotherId = anotherObject.Strategy.ObjectId;
                var anotherProxy = C1.Instantiate(this.Transaction, anotherId);

                Assert.Equal(anObject, proxy);
                Assert.Equal(anotherObject, anotherProxy);
                Assert.NotEqual(anObject, anotherObject);
                Assert.NotEqual(anObject, anotherProxy);
                Assert.NotEqual(proxy, anotherObject);
                Assert.NotEqual(proxy, anotherProxy);

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                anotherObject = C1.Create(this.Transaction);
                anotherId = anotherObject.Strategy.ObjectId;
                anotherProxy = C1.Instantiate(this.Transaction, anotherId);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                anotherObject = C1.Instantiate(this.Transaction, anotherId);

                Assert.Equal(anObject, proxy);
                Assert.Equal(anotherObject, anotherProxy);
                Assert.NotEqual(anObject, anotherObject);
                Assert.NotEqual(anObject, anotherProxy);
                Assert.NotEqual(proxy, anotherObject);
                Assert.NotEqual(proxy, anotherProxy);

                //// Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                anotherObject = C1.Create(this.Transaction);
                anotherId = anotherObject.Strategy.ObjectId;
                anotherProxy = C1.Instantiate(this.Transaction, anotherId);

                this.Transaction.Rollback();

                Assert.Equal(anObject, proxy);
                Assert.Equal(anotherObject, anotherProxy);
                Assert.NotEqual(anObject, anotherObject);
                Assert.NotEqual(anObject, anotherProxy);
                Assert.NotEqual(proxy, anotherObject);
                Assert.NotEqual(proxy, anotherProxy);
            }
        }

        [Fact]
        public void Instantiate()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var anObject = C1.Create(this.Transaction);
                var id = anObject.Strategy.ObjectId;
                var sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Commit();

                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Commit();

                sameObject = (C1)this.Transaction.Instantiate(id);
                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Commit();

                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Commit();

                sameObject = (C1)this.Transaction.Instantiate(id);
                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                //// Proxy

                //// Unit

                var subject = C1.Create(this.Transaction);
                id = subject.Strategy.ObjectId;
                this.Transaction.Commit();

                subject.C1AllorsString = "a";
                var proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                Assert.Equal("b", subject.C1AllorsString);
                Assert.Equal("b", proxy.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                Assert.Equal("d", proxy.C1AllorsString);
                Assert.Equal("d", subject.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                this.Transaction.Commit();
                Assert.Equal("b", subject.C1AllorsString);
                Assert.Equal("b", proxy.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                this.Transaction.Commit();
                Assert.Equal("d", proxy.C1AllorsString);
                Assert.Equal("d", subject.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                Assert.Equal("b", subject.C1AllorsString);
                this.Transaction.Commit();
                Assert.Equal("b", proxy.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                Assert.Equal("d", proxy.C1AllorsString);
                this.Transaction.Commit();
                Assert.Equal("d", subject.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                this.Transaction.Commit();
                Assert.Equal("b", subject.C1AllorsString);
                this.Transaction.Commit();
                Assert.Equal("b", proxy.C1AllorsString);
                this.Transaction.Commit();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                this.Transaction.Commit();
                Assert.Equal("d", proxy.C1AllorsString);
                this.Transaction.Commit();
                Assert.Equal("d", subject.C1AllorsString);
                this.Transaction.Commit();

                //// IComposite

                var fromProxy = C1.Create(this.Transaction);
                var toProxy = C1.Create(this.Transaction);
                var fromId = fromProxy.Strategy.ObjectId;
                var toId = toProxy.Strategy.ObjectId;
                this.Transaction.Commit();

                var from = C1.Instantiate(this.Transaction, fromId);
                var to = C1.Instantiate(this.Transaction, toId);
                from.C1AllorsString = "a";
                from.C1C1one2one = to;
                from.C1C1many2one = to;
                from.AddC1C1one2many(to);
                from.AddC1C1many2many(to);

                StrategyAssert.RolesExistExclusive(
                    fromProxy,
                    m.C1.C1AllorsString,
                    m.C1.C1C1one2one,
                    m.C1.C1C1many2one,
                    m.C1.C1C1one2manies,
                    m.C1.C1C1many2manies);

                StrategyAssert.AssociationsExistExclusive(
                    toProxy,
                    m.C1.C1WhereC1C1one2one,
                    m.C1.C1sWhereC1C1many2one,
                    m.C1.C1WhereC1C1one2many,
                    m.C1.C1sWhereC1C1many2many);

                Assert.Equal("a", fromProxy.C1AllorsString);
                Assert.Equal(toProxy, fromProxy.C1C1one2one);
                Assert.Equal(toProxy, fromProxy.C1C1many2one);
                Assert.Contains(toProxy, fromProxy.C1C1one2manies);
                Assert.Contains(toProxy, fromProxy.C1C1many2manies);

                Assert.Equal(fromProxy, toProxy.C1WhereC1C1one2one);
                Assert.Contains(fromProxy, toProxy.C1sWhereC1C1many2one);
                Assert.Equal(fromProxy, toProxy.C1WhereC1C1one2many);
                Assert.Contains(fromProxy, toProxy.C1sWhereC1C1many2many);

                from.C1AllorsString = null;
                from.C1C1one2one = null;
                from.C1C1many2one = null;
                from.C1C1one2manies = null;
                from.C1C1many2manies = null;

                this.Transaction.Commit();

                Assert.Null(from.C1AllorsString);
                Assert.Null(from.C1C1one2one);
                Assert.Null(from.C1C1many2one);
                Assert.Empty(from.C1C1one2manies);
                Assert.Empty(from.C1C1many2manies);

                StrategyAssert.RolesExistExclusive(from);
                StrategyAssert.AssociationsExistExclusive(to);

                Assert.Null(fromProxy.C1AllorsString);
                Assert.Null(fromProxy.C1C1one2one);
                Assert.Null(fromProxy.C1C1many2one);
                Assert.Empty(fromProxy.C1C1one2manies);
                Assert.Empty(fromProxy.C1C1many2manies);

                StrategyAssert.RolesExistExclusive(fromProxy);
                StrategyAssert.AssociationsExistExclusive(toProxy);

                this.Transaction.Commit();

                //// Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));
                this.Transaction.Commit();

                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Rollback();

                sameObject = (C1)this.Transaction.Instantiate(id);
                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Rollback();

                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                this.Transaction.Rollback();

                sameObject = (C1)this.Transaction.Instantiate(id);
                sameObject = (C1)this.Transaction.Instantiate(id);

                Assert.True(anObject.Equals(sameObject));
                Assert.True(anObject.Strategy.ObjectId.Equals(sameObject.Strategy.ObjectId));

                //// Proxy

                //// Unit

                subject = C1.Create(this.Transaction);
                id = subject.Strategy.ObjectId;
                this.Transaction.Commit();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                Assert.Equal("b", subject.C1AllorsString);
                Assert.Equal("b", proxy.C1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                Assert.Equal("d", proxy.C1AllorsString);
                Assert.Equal("d", subject.C1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                this.Transaction.Rollback();
                Assert.False(subject.ExistC1AllorsString);
                Assert.False(proxy.ExistC1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                this.Transaction.Rollback();
                Assert.False(proxy.ExistC1AllorsString);
                Assert.False(subject.ExistC1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                Assert.Equal("b", subject.C1AllorsString);
                this.Transaction.Rollback();
                Assert.False(proxy.ExistC1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                Assert.Equal("d", proxy.C1AllorsString);
                this.Transaction.Rollback();
                Assert.False(subject.ExistC1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "a";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "b";
                this.Transaction.Rollback();
                Assert.False(subject.ExistC1AllorsString);
                this.Transaction.Rollback();
                Assert.False(proxy.ExistC1AllorsString);
                this.Transaction.Rollback();

                subject.C1AllorsString = "c";
                proxy = C1.Instantiate(this.Transaction, id);
                proxy.C1AllorsString = "d";
                this.Transaction.Rollback();
                Assert.False(proxy.ExistC1AllorsString);
                this.Transaction.Rollback();
                Assert.False(subject.ExistC1AllorsString);
                this.Transaction.Rollback();

                //// IComposite

                from = C1.Instantiate(this.Transaction, fromId);
                to = C1.Instantiate(this.Transaction, toId);
                from.C1AllorsString = "a";
                from.C1C1one2one = to;
                from.C1C1many2one = to;
                from.AddC1C1one2many(to);
                from.AddC1C1many2many(to);

                StrategyAssert.RolesExistExclusive(
                    fromProxy,
                    m.C1.C1AllorsString,
                    m.C1.C1C1one2one,
                    m.C1.C1C1many2one,
                    m.C1.C1C1one2manies,
                    m.C1.C1C1many2manies);

                StrategyAssert.AssociationsExistExclusive(
                    toProxy,
                    m.C1.C1WhereC1C1one2one,
                    m.C1.C1sWhereC1C1many2one,
                    m.C1.C1WhereC1C1one2many,
                    m.C1.C1sWhereC1C1many2many);

                Assert.Equal("a", fromProxy.C1AllorsString);
                Assert.Equal(toProxy, fromProxy.C1C1one2one);
                Assert.Equal(toProxy, fromProxy.C1C1many2one);
                Assert.Contains(toProxy, fromProxy.C1C1one2manies);
                Assert.Contains(toProxy, fromProxy.C1C1many2manies);

                Assert.Equal(fromProxy, toProxy.C1WhereC1C1one2one);
                Assert.Contains(fromProxy, toProxy.C1sWhereC1C1many2one);
                Assert.Equal(fromProxy, toProxy.C1WhereC1C1one2many);
                Assert.Contains(fromProxy, toProxy.C1sWhereC1C1many2many);

                this.Transaction.Rollback();

                Assert.Null(from.C1AllorsString);
                Assert.Null(from.C1C1one2one);
                Assert.Null(from.C1C1many2one);
                Assert.Empty(from.C1C1one2manies);
                Assert.Empty(from.C1C1many2manies);

                StrategyAssert.RolesExistExclusive(from);
                StrategyAssert.AssociationsExistExclusive(to);

                Assert.Null(fromProxy.C1AllorsString);
                Assert.Null(fromProxy.C1C1one2one);
                Assert.Null(fromProxy.C1C1many2one);
                Assert.Empty(fromProxy.C1C1one2manies);
                Assert.Empty(fromProxy.C1C1many2manies);

                StrategyAssert.RolesExistExclusive(fromProxy);
                StrategyAssert.AssociationsExistExclusive(toProxy);

                this.Transaction.Rollback();

                var unexistingObject = (C1)this.Transaction.Instantiate("1000000");
                Assert.Null(unexistingObject);

                unexistingObject = (C1)this.Transaction.Instantiate("");
                Assert.Null(unexistingObject);

                unexistingObject = (C1)this.Transaction.Instantiate(" ");
                Assert.Null(unexistingObject);

                unexistingObject = (C1)this.Transaction.Instantiate("\t");
                Assert.Null(unexistingObject);

                unexistingObject = (C1)this.Transaction.Instantiate("blah blah blah");
                Assert.Null(unexistingObject);
            }
        }

        [Fact]
        public void InstantiateMany()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                int[] runs = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };

                foreach (var run in runs)
                {
                    // Empty arrays
                    long[] nullObjectIdArray = null;
                    var allorsObjects = this.Transaction.Instantiate(nullObjectIdArray);
                    Assert.Empty(allorsObjects);

                    string[] nullStringArray = null;
                    allorsObjects = this.Transaction.Instantiate(nullStringArray);
                    Assert.Empty(allorsObjects);

                    IObject[] nullObjectArray = null;
                    allorsObjects = this.Transaction.Instantiate(nullObjectArray);
                    Assert.Empty(allorsObjects);

                    var objects = new IObject[run];
                    var idStrings = new string[run];
                    var ids = new long[run];
                    for (var i = 0; i < run; i++)
                    {
                        var anObject = C1.Create(this.Transaction);
                        objects[i] = anObject;
                        idStrings[i] = anObject.Strategy.ObjectId.ToString();
                        ids[i] = anObject.Strategy.ObjectId;
                    }

                    this.Transaction.Commit();

                    allorsObjects = this.Transaction.Instantiate(objects);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    allorsObjects = this.Transaction.Instantiate(idStrings);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    allorsObjects = this.Transaction.Instantiate(ids);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Commit();

                    allorsObjects = this.Transaction.Instantiate(objects);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Commit();

                    allorsObjects = this.Transaction.Instantiate(idStrings);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Commit();

                    allorsObjects = this.Transaction.Instantiate(ids);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Rollback();

                    allorsObjects = this.Transaction.Instantiate(objects);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Rollback();

                    allorsObjects = this.Transaction.Instantiate(idStrings);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Rollback();

                    allorsObjects = this.Transaction.Instantiate(ids);

                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Equal(allorsObjects[i].Id, ids[i]);
                    }

                    this.Transaction.Commit();

                    // Caching in Sql
                    this.SwitchDatabase();
                    var minusOne = new List<long>(ids);
                    minusOne.RemoveAt(0);
                    allorsObjects = this.Transaction.Instantiate(minusOne.ToArray());

                    allorsObjects = this.Transaction.Instantiate(ids);
                    Assert.Equal(run, allorsObjects.Length);
                    for (var i = 0; i < allorsObjects.Length; i++)
                    {
                        Assert.Contains(allorsObjects[i].Id, ids);
                    }

                    this.Transaction.Commit();

                    Assert.Empty(this.Transaction.Instantiate(Array.Empty<IObject>()));

                    this.Transaction.Commit();

                    Assert.Empty(this.Transaction.Instantiate(Array.Empty<string>()));

                    this.Transaction.Commit();

                    Assert.Empty(this.Transaction.Instantiate(Array.Empty<long>()));

                    this.Transaction.Commit();

                    var doesntExistIds = new[] { (1000 * 1000 * 1000).ToString() };

                    Assert.Empty(this.Transaction.Instantiate(doesntExistIds));

                    // Preserve order
                    var c1A = C1.Create(this.Transaction);
                    var c1B = C1.Create(this.Transaction);
                    var c1C = C1.Create(this.Transaction);
                    var c1D = C1.Create(this.Transaction);

                    var objectIds = new[] { c1A.Id, c1B.Id, c1C.Id, c1D.Id };

                    var instantiatedObjects = this.Transaction.Instantiate(objectIds);

                    Assert.Equal(4, instantiatedObjects.Length);
                    Assert.Equal(c1A, instantiatedObjects[0]);
                    Assert.Equal(c1B, instantiatedObjects[1]);
                    Assert.Equal(c1C, instantiatedObjects[2]);
                    Assert.Equal(c1D, instantiatedObjects[3]);

                    this.Transaction.Commit();

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c1C = (C1)transaction2.Instantiate(objectIds[2]);

                        instantiatedObjects = transaction2.Instantiate(objectIds);

                        Assert.Equal(4, instantiatedObjects.Length);
                        Assert.Equal(c1A, instantiatedObjects[0]);
                        Assert.Equal(c1B, instantiatedObjects[1]);
                        Assert.Equal(c1C, instantiatedObjects[2]);
                        Assert.Equal(c1D, instantiatedObjects[3]);
                    }
                }
            }
        }

        [Fact]
        public void IsDeleted()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                //// Commit + Commit

                var anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.False(anObject.Strategy.IsDeleted);

                //// Commit + Rollback

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.False(anObject.Strategy.IsDeleted);

                //// Rollback + Commit

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                //// Rollback + Rollback

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                //// With Delete

                //// Commit + Commit

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                //// Commit + Rollback

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                //// Rollback + Commit

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Commit();
                Assert.True(anObject.Strategy.IsDeleted);

                //// Rollback + Rollback

                anObject = C1.Create(this.Transaction);
                Assert.False(anObject.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(anObject.Strategy.IsDeleted);

                //// Proxy

                //// Without Delete

                //// Commit + Commit

                anObject = C1.Create(this.Transaction);
                var id = anObject.Strategy.ObjectId;
                var proxy = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                //// Commit + Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                //// Rollback + Commit

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                //// Rollback + Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);
                Assert.False(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                //// With Delete

                //// Commit + Commit

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                Assert.False(proxy.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                //// Commit + Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                Assert.False(proxy.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                Assert.True(proxy.Strategy.IsDeleted);

                //// Rollback + Commit

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                Assert.False(proxy.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Commit();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                //// Rollback + Rollback

                anObject = C1.Create(this.Transaction);
                id = anObject.Strategy.ObjectId;
                proxy = C1.Instantiate(this.Transaction, id);

                Assert.False(proxy.Strategy.IsDeleted);

                anObject.Strategy.Delete();

                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);

                this.Transaction.Rollback();
                anObject = C1.Instantiate(this.Transaction, id);
                Assert.True(proxy.Strategy.IsDeleted);
            }
        }

        [Fact]
        public void OnChange()
        {
            // TODO:
        }

        [Fact]
        public void Rollback()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                const int ObjectCount = 10;
                var allorsObjects = this.Transaction.Create(m.Company, ObjectCount);
                var ids = new string[ObjectCount];
                for (var i = 0; i < ObjectCount; i++)
                {
                    var allorsObject = allorsObjects[i];
                    ids[i] = allorsObject.Strategy.ObjectId.ToString();
                }

                Assert.Equal(ObjectCount, allorsObjects.Length);

                this.Transaction.Rollback();

                allorsObjects = this.Transaction.Instantiate(ids);

                Assert.Empty(allorsObjects);
            }
        }

        [Fact]
        public void Versioning()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var obj = this.Transaction.Create<C1>();

                Assert.Equal(2, obj.Strategy.ObjectVersion);

                this.Transaction.Commit();

                using (var transaction2 = this.CreateTransaction())
                {
                    Assert.Equal(2, transaction2.Instantiate(obj).Strategy.ObjectVersion);
                }

                Assert.Equal(2, obj.Strategy.ObjectVersion);

                obj.C1AllorsString = "Changed";

                Assert.Equal(2, obj.Strategy.ObjectVersion);

                this.Transaction.Commit();

                using (var transaction2 = this.CreateTransaction())
                {
                    var transaction2Object = (C1)transaction2.Instantiate(obj);
                    Assert.Equal(3, transaction2Object.Strategy.ObjectVersion);
                    transaction2Object.C1AllorsString = "Transaction 2 changed";
                    transaction2.Commit();

                    Assert.Equal(4, transaction2Object.Strategy.ObjectVersion);
                }

                this.Transaction.Rollback();

                Assert.Equal(4, obj.Strategy.ObjectVersion);

                obj.C1AllorsString = "Changed again.";

                Assert.Equal(4, obj.Strategy.ObjectVersion);

                this.Transaction.Commit();

                using (var transaction2 = this.CreateTransaction())
                {
                    Assert.Equal(5, transaction2.Instantiate(obj).Strategy.ObjectVersion);
                }

                Assert.Equal(5, obj.Strategy.ObjectVersion);

                obj.RemoveC1AllorsString();

                Assert.Equal(5, obj.Strategy.ObjectVersion);

                this.Transaction.Commit();

                using (var transaction2 = this.CreateTransaction())
                {
                    Assert.Equal(6, transaction2.Instantiate(obj).Strategy.ObjectVersion);
                }

                Assert.Equal(6, obj.Strategy.ObjectVersion);
            }
        }

        [Fact]
        public void WihthoutRoles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // TODO: Move to other tests
                var withoutValueRoles = ClassWithoutUnitRoles.Create(this.Transaction);
                var withoutValueRolesClone = (ClassWithoutUnitRoles)this.GetExtent(m.ClassWithoutUnitRoles)[0];

                Assert.Equal(withoutValueRoles, withoutValueRolesClone);

                var withoutRoles = ClassWithoutRoles.Create(this.Transaction);
                var withoutRolesClone = (ClassWithoutRoles)this.GetExtent(m.ClassWithoutRoles)[0];

                Assert.Equal(withoutRoles, withoutRolesClone);
            }
        }

        [Fact]
        public void SwitchPopulation()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);
                var c2A = C2.Create(this.Transaction);
                var c2B = C2.Create(this.Transaction);
                var c2C = C2.Create(this.Transaction);

                var c1aId = c1A.Id.ToString();

                c1A.C1I12one2one = c2A;
                c1A.AddC1I12one2many(c2B);
                c1A.AddC1I12one2many(c2C);

                this.Transaction.Commit();

                this.SwitchDatabase();

                var switchC1A = (C1)this.Transaction.Instantiate(c1A.Id.ToString());

                Assert.Equal(m.C1, switchC1A.Strategy.Class);

                var switchC2A = switchC1A.C1I12one2one;

                Assert.Equal(m.C2, switchC2A.Strategy.Class);

                var switchC2BC = switchC1A.C1I12one2manies;

                Assert.Equal(m.C2, switchC2BC.ElementAt(0).Strategy.Class);
                Assert.Equal(m.C2, switchC2BC.ElementAt(1).Strategy.Class);

                this.Transaction.Commit();

                this.SwitchDatabase();

                long[] objectIds = { c1A.Id, c2A.Id };
                var switchC1aC2a = this.Transaction.Instantiate(objectIds);

                Assert.Equal(2, switchC1aC2a.Length);
                Assert.Contains(this.Transaction.Instantiate(c1A.Id), switchC1aC2a);
                Assert.Contains(this.Transaction.Instantiate(c2A.Id), switchC1aC2a);
            }
        }

        [Fact]
        public void SwitchTransaction()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                if (this.Transaction is ITransaction)
                {
                    var c1A = C1.Create(this.Transaction);
                    var c2A = C2.Create(this.Transaction);

                    var c1AObjectId = c1A.Id;
                    var c2AObjectId = c2A.Id;

                    c1A.C1C2one2one = c2A;
                    c1A.C1C2many2one = c2A;
                    c1A.AddC1C2one2many(c2A);
                    c1A.AddC1C2many2many(c2A);

                    this.Transaction.Commit();

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c2A = (C2)transaction2.Instantiate(c2AObjectId);

                        Assert.True(c2A.ExistC1WhereC1C2one2one);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c2A = (C2)transaction2.Instantiate(c2AObjectId);

                        Assert.True(c2A.ExistC1sWhereC1C2many2one);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c2A = (C2)transaction2.Instantiate(c2AObjectId);

                        Assert.True(c2A.ExistC1WhereC1C2one2many);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c2A = (C2)transaction2.Instantiate(c2AObjectId);

                        Assert.True(c2A.ExistC1sWhereC1C2many2many);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c1A = (C1)transaction2.Instantiate(c1AObjectId);

                        Assert.True(c1A.ExistC1C2one2one);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c1A = (C1)transaction2.Instantiate(c1AObjectId);

                        Assert.True(c1A.ExistC1C2many2one);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c1A = (C1)transaction2.Instantiate(c1AObjectId);

                        Assert.True(c1A.ExistC1C2one2manies);
                    }

                    using (var transaction2 = this.CreateTransaction())
                    {
                        c1A = (C1)transaction2.Instantiate(c1AObjectId);

                        Assert.True(c1A.ExistC1C2many2manies);
                    }
                }
            }
        }

        [Fact]
        public virtual void CreateManyPopulations()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // don't garbage collect populations
                var populations = new List<IDatabase>();

                for (var i = 0; i < 100; i++)
                {
                    var population1 = this.CreatePopulation();
                    populations.Add(population1);
                    var transaction1 = population1.CreateTransaction();

                    var c1 = transaction1.Create<C1>();

                    var population2 = this.CreatePopulation();
                    populations.Add(population2);
                    var transaction2 = population2.CreateTransaction();

                    var c2 = transaction2.Create<C2>();

                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }

        [Fact]
        public void ObjectType()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1a = this.Transaction.Create(m.C1);
                Assert.Equal(m.C1, c1a.Strategy.Class);

                this.Transaction.Commit();

                Assert.Equal(m.C1, c1a.Strategy.Class);

                var c1b = this.Transaction.Create(m.C1);

                this.Transaction.Rollback();

                var exceptionThrown = false;

                try
                {
                    var objectType = c1b.Strategy.Class;
                }
                catch
                {
                    exceptionThrown = true;
                }

                Assert.True(exceptionThrown);

                var c2a = this.Transaction.Create(m.C2);
                Assert.Equal(m.C2, c2a.Strategy.Class);

                this.Transaction.Commit();

                Assert.Equal(m.C2, c2a.Strategy.Class);
            }
        }

        [Fact]
        public void PrefetchEmptyPolicy()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);
                    c1A.C1AllorsString = "1";

                    var prefetchPolicy = new PrefetchPolicyBuilder().Build();

                    this.Transaction.Prefetch(prefetchPolicy, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    }
                }
            }
        }

        [Fact]
        public void PrefetchUnitRole()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);
                    c1A.C1AllorsString = "1";

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal("1", c1A.C1AllorsString);

                    this.Transaction.Commit();

                    Assert.Equal("1", c1A.C1AllorsString);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal("1", c1A.C1AllorsString);

                    this.Transaction.Commit();

                    c1A.C1AllorsString = "2";

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal("2", c1A.C1AllorsString);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1AllorsString }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal("1", c1A.C1AllorsString);

                    this.Transaction.Rollback();

                    Assert.Equal("1", c1A.C1AllorsString);
                }
            }
        }

        [Fact]
        public void PrefetchCompositeRoleOne2One()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);
                    var c2A = C2.Create(this.Transaction);
                    var c2b = C2.Create(this.Transaction);

                    c1A.C1C2one2one = c2A;

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal(c2A, c1A.C1C2one2one);

                    this.Transaction.Commit();

                    Assert.Equal(c2A, c1A.C1C2one2one);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal(c2A, c1A.C1C2one2one);

                    this.Transaction.Commit();

                    c1A.C1C2one2one = c2b;

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal(c2b, c1A.C1C2one2one);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2one }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Equal(c2A, c1A.C1C2one2one);

                    this.Transaction.Rollback();

                    Assert.Equal(c2A, c1A.C1C2one2one);
                }
            }
        }

        [Fact]
        public void PrefetchCompositeRoleOne2OneAndUnit()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.C1.C1C2one2one, new IPropertyType[] { m.C2.C2AllorsString })
                        .Build();

                    var c1a = C1.Create(this.Transaction);
                    var c2a = C2.Create(this.Transaction);
                    var c2b = C2.Create(this.Transaction);

                    c2a.C2AllorsString = "c2a";
                    c2b.C2AllorsString = "c2b";

                    c1a.C1C2one2one = c2a;

                    this.Transaction.Prefetch(prefetchPolicy, c1a);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1a);
                    }

                    Assert.Equal(c2a, c1a.C1C2one2one);

                    this.Transaction.Commit();

                    Assert.Equal(c2a, c1a.C1C2one2one);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1a);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1a);
                    }

                    Assert.Equal(c2a, c1a.C1C2one2one);

                    this.Transaction.Commit();

                    c1a.C1C2one2one = c2b;

                    this.Transaction.Prefetch(prefetchPolicy, c1a);

                    Assert.Equal(c2b, c1a.C1C2one2one);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(prefetchPolicy, c1a);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1a);
                    }

                    Assert.Equal(c2a, c1a.C1C2one2one);

                    this.Transaction.Rollback();

                    Assert.Equal(c2a, c1a.C1C2one2one);
                }
            }
        }

        [Fact]
        public void PrefetchCompositeRoleOne2OneEmptyAndUnit()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.C1.C1C2one2one, new IPropertyType[] { m.C2.C2AllorsString })
                        .Build();

                    var c1A = C1.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRolesOne2Many()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);
                    var c2A = C2.Create(this.Transaction);
                    var c2b = C2.Create(this.Transaction);
                    c1A.AddC1C2one2many(c2A);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });

                    Assert.Contains(c2A, c1A.C1C2one2manies);

                    this.Transaction.Commit();

                    Assert.Contains(c2A, c1A.C1C2one2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2A, c1A.C1C2one2manies);

                    this.Transaction.Commit();

                    c1A.RemoveC1C2one2many(c2A);
                    c1A.AddC1C2one2many(c2b);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2b, c1A.C1C2one2manies);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2A, c1A.C1C2one2manies);

                    this.Transaction.Rollback();

                    Assert.Contains(c2A, c1A.C1C2one2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRoleOne2ManyEmpty()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2one2manies }, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRoleOne2ManyEmptyAndUnit()
        {

            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.C1.C1C2one2manies, new IPropertyType[] { m.C2.C2AllorsString })
                        .Build();

                    var c1A = C1.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2one2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRoleMany2Many()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);
                    var c2A = C2.Create(this.Transaction);
                    var c2b = C2.Create(this.Transaction);
                    c1A.AddC1C2many2many(c2A);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2A, c1A.C1C2many2manies);

                    this.Transaction.Commit();

                    Assert.Contains(c2A, c1A.C1C2many2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2A, c1A.C1C2many2manies);

                    this.Transaction.Commit();

                    c1A.RemoveC1C2many2many(c2A);
                    c1A.AddC1C2many2many(c2b);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2b, c1A.C1C2many2manies);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, new[] { c1A.Strategy.ObjectId });
                    }

                    Assert.Contains(c2A, c1A.C1C2many2manies);

                    this.Transaction.Rollback();

                    Assert.Contains(c2A, c1A.C1C2many2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRoleMany2ManyEmpty()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1A = C1.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, c1A);
                    }

                    Assert.Empty(c1A.C1C2many2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C1.C1C2many2manies }, c1A);
                    }

                    Assert.Empty(c1A.C1C2many2manies);
                }
            }
        }

        [Fact]
        public void PrefetchCompositesRoleMany2ManyEmptyAndUnit()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.C1.C1C2many2manies, new IPropertyType[] { m.C2.C2AllorsString })
                        .Build();

                    var c1A = C1.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2many2manies);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c1A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c1A);
                    }

                    Assert.Empty(c1A.C1C2many2manies);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationOne2One()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1a = C1.Create(this.Transaction);
                    var c1b = C1.Create(this.Transaction);
                    var c2a = C2.Create(this.Transaction);
                    c1a.C1C2one2one = c2a;

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });

                    Assert.Equal(c1a, c2a.C1WhereC1C2one2one);

                    this.Transaction.Commit();

                    Assert.Equal(c1a, c2a.C1WhereC1C2one2one);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Equal(c1a, c2a.C1WhereC1C2one2one);

                    this.Transaction.Commit();

                    c1b.C1C2one2one = c2a;

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Equal(c1b, c2a.C1WhereC1C2one2one);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Equal(c1a, c2a.C1WhereC1C2one2one);

                    this.Transaction.Rollback();

                    Assert.Equal(c1a, c2a.C1WhereC1C2one2one);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationOne2OneEmpty()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1WhereC1C2one2one }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationOne2OneEmptyAndUnit()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder().WithRule(m.C2.C1WhereC1C2one2one.RoleType, new IPropertyType[] { m.C1.C1AllorsString }).Build();

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationMany2Many()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c1a = C1.Create(this.Transaction);
                    var c1b = C1.Create(this.Transaction);
                    var c2a = C2.Create(this.Transaction);
                    c1a.AddC1C2many2many(c2a);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Contains(c1a, c2a.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    Assert.Contains(c1a, c2a.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Contains(c1a, c2a.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    c1a.RemoveC1C2many2many(c2a);
                    c1b.AddC1C2many2many(c2a);

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    }

                    var b = c1b.Strategy.ObjectId;
                    var assoc = c2a.C1sWhereC1C2many2many.ElementAt(0).Strategy.ObjectId;

                    Assert.Contains(c1b, c2a.C1sWhereC1C2many2many);

                    this.Transaction.Rollback();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, new[] { c2a.Strategy.ObjectId });
                    }

                    Assert.Contains(c1a, c2a.C1sWhereC1C2many2many);

                    this.Transaction.Rollback();

                    Assert.Contains(c1a, c2a.C1sWhereC1C2many2many);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationMany2ManyEmpty()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2many }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationMany2ManyEmptyAndUnit()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder().WithRule(m.C2.C1sWhereC1C2many2many, new IPropertyType[] { m.C1.C1AllorsString }).Build();

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2many);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationMany2OneEmpty()
        {
            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2one }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2one }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2one);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2one }, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(new IPropertyType[] { m.C2.C1sWhereC1C2many2one }, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2one);
                }
            }
        }

        [Fact]
        public void PrefetchAssociationMany2OneEmptyAndUnit()
        {

            foreach (var twice in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var prefetchPolicy = new PrefetchPolicyBuilder()
                        .WithRule(m.C2.C1sWhereC1C2many2one, new IPropertyType[] { m.C1.C1AllorsString })
                        .Build();

                    var c2A = C2.Create(this.Transaction);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2one);

                    this.Transaction.Commit();

                    this.Transaction.Prefetch(prefetchPolicy, c2A);
                    if (twice)
                    {
                        this.Transaction.Prefetch(prefetchPolicy, c2A);
                    }

                    Assert.Empty(c2A.C1sWhereC1C2many2one);
                }
            }
        }

        protected abstract void SwitchDatabase();

        protected abstract IDatabase CreatePopulation();

        protected abstract ITransaction CreateTransaction();

        private IObject[] GetExtent(IComposite objectType) => this.Transaction.Extent(objectType);
    }
}
