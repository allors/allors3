// <copyright file="LifeCycleTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public abstract class LifeCycleTest : IDisposable
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
        public void InstantiateNull()
        {
            foreach (var init in this.Inits)
            {
                init();

                IObject nullObject = null;
                Assert.Null(this.Transaction.Instantiate(nullObject));
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

        protected static void AssertReadVisibilityAcrossCommit(IDatabase readerDatabase, IDatabase writerDatabase, bool retainsSnapshot)
        {
            writerDatabase.Init();

            long sourceId;
            long targetId;
            using (var setup = writerDatabase.CreateTransaction())
            {
                var source = setup.Create<C1>();
                source.C1AllorsString = "Before commit";
                var target = setup.Create<C2>();
                sourceId = source.Id;
                targetId = target.Id;
                setup.Commit();
            }

            using var reader = readerDatabase.CreateTransaction();
            var initialMember = Assert.Single(reader.Extent<C1>().ToArray());
            Assert.Equal(sourceId, initialMember.Id);
            var initialVersion = initialMember.Strategy.ObjectVersion;
            var targetInReader = (C2)reader.Instantiate(targetId);
            var targetVersion = targetInReader.Strategy.ObjectVersion;

            long addedId;
            using (var writer = writerDatabase.CreateTransaction())
            {
                Assert.NotSame(reader, writer);
                var source = (C1)writer.Instantiate(sourceId);
                source.C1AllorsString = "After commit";
                var target = (C2)writer.Instantiate(targetId);
                var added = writer.Create<C1>();
                added.C1C2many2one = target;
                addedId = added.Id;
                writer.Commit();

                Assert.True(source.Strategy.ObjectVersion > initialVersion);
                Assert.Equal(targetVersion, target.Strategy.ObjectVersion);
            }

            // Neither this unit role nor the inverse relation was read before the other
            // transaction committed. Fresh extents also have to use the same read scope.
            Assert.Equal(retainsSnapshot ? "Before commit" : "After commit", initialMember.C1AllorsString);
            Assert.Equal(initialVersion, initialMember.Strategy.ObjectVersion);
            Assert.Equal(
                retainsSnapshot ? Array.Empty<long>() : new[] { addedId },
                targetInReader.C1sWhereC1C2many2one.Select(v => v.Id).ToArray());
            Assert.Equal(targetVersion, targetInReader.Strategy.ObjectVersion);
            Assert.Equal(
                retainsSnapshot ? new[] { sourceId } : new[] { sourceId, addedId },
                reader.Extent<C1>().ToArray().Select(v => v.Id).OrderBy(v => v).ToArray());

            using var verification = writerDatabase.CreateTransaction();
            var currentSource = (C1)verification.Instantiate(sourceId);
            Assert.Equal("After commit", currentSource.C1AllorsString);
            Assert.True(currentSource.Strategy.ObjectVersion > initialVersion);
            Assert.Equal(new[] { addedId }, ((C2)verification.Instantiate(targetId)).C1sWhereC1C2many2one.Select(v => v.Id).ToArray());
            Assert.Equal(2, verification.Extent<C1>().ToArray().Length);
        }

        protected static void AssertProtectedReadRetainsSnapshot(Sql.Database writerDatabase, Sql.Database readerDatabase)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            using var scope = readerDatabase.OpenRead();
            Assert.Equal(writerDatabase.PopulationLifecycle.RequireReady().Incarnation, scope.Population.Incarnation);
            Assert.Equal(Sql.PopulationState.Ready, scope.Population.State);

            // No domain read has run yet: lifecycle lookup must already have fixed this snapshot.
            long addedId;
            using (var writer = writerDatabase.CreateTransaction())
            {
                var source = (C1)writer.Instantiate(baseline.SourceId);
                source.C1AllorsString = "Committed after read admission";
                var added = writer.Create<C1>();
                added.C1C2many2one = (C2)writer.Instantiate(baseline.TargetId);
                addedId = added.Id;
                writer.Commit();
            }

            var transaction = scope.Transaction;
            var sourceInRead = Assert.IsType<C1>(transaction.Instantiate(baseline.SourceId));
            var targetInRead = Assert.IsType<C2>(transaction.Instantiate(baseline.TargetId));
            Assert.Equal("Committed baseline", sourceInRead.C1AllorsString);
            Assert.Equal(baseline.SourceVersion, sourceInRead.Strategy.ObjectVersion);
            Assert.Equal(new[] { baseline.SourceId }, transaction.Extent<C1>().ToArray().Select(v => v.Id).ToArray());
            Assert.Empty(targetInRead.C1sWhereC1C2many2one);
            Assert.Null(transaction.Instantiate(addedId));
            scope.EnsureValid();

            using var current = writerDatabase.CreateTransaction();
            Assert.Equal("Committed after read admission", ((C1)current.Instantiate(baseline.SourceId)).C1AllorsString);
            Assert.Equal(2, current.Extent<C1>().Count);
            Assert.Equal(new[] { addedId }, ((C2)current.Instantiate(baseline.TargetId)).C1sWhereC1C2many2one.Select(v => v.Id).ToArray());
        }

        protected static void AssertProtectedReadRejectsMutation(Sql.Database writerDatabase, Sql.Database readerDatabase, string operation)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            using var scope = readerDatabase.OpenRead();
            var transaction = scope.Transaction;
            var source = Assert.IsType<C1>(transaction.Instantiate(baseline.SourceId));
            var target = Assert.IsType<C2>(transaction.Instantiate(baseline.TargetId));
            var m = writerDatabase.Context().M;
            scope.EnsureValid();
            Action forbidden = operation switch
            {
                "create" => () => transaction.Create<C1>(),
                "create-many" => () => transaction.Create<C1>(2),
                "set-unit" => () => source.C1AllorsString = "Forbidden",
                "remove-unit" => () => source.Strategy.RemoveUnitRole(m.C1.C1AllorsString),
                "set-composite" => () => source.C1C2many2one = target,
                "remove-composite" => () => source.Strategy.RemoveCompositeRole(m.C1.C1C2many2one),
                "add-composites" => () => source.Strategy.AddCompositesRole(m.C1.C1C2many2manies, target),
                "remove-composites" => () => source.Strategy.RemoveCompositesRole(m.C1.C1C2many2manies, target),
                "set-composites" => () => source.Strategy.SetCompositesRole(m.C1.C1C2many2manies, new IObject[] { target }),
                "clear-composites" => () => source.Strategy.RemoveCompositesRole(m.C1.C1C2many2manies),
                "delete" => () => source.Strategy.Delete(),
                "commit" => () => transaction.Commit(),
                "rollback" => () => transaction.Rollback(),
                "dispose-transaction" => () => transaction.Dispose(),
                "connection-command" => () => transaction.Connection.CreateCommand(),
                "connection-commit" => () => transaction.Connection.Commit(),
                "connection-rollback" => () => transaction.Connection.Rollback(),
                "require-flush" => () => transaction.State.RequireFlush((Sql.Strategy)source.Strategy),
                "new-state" => () => new Sql.State(transaction),
                "services-init" => () => transaction.Services.OnInit(transaction),
                "services-dispose" => () => transaction.Services.Dispose(),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };

            Assert.ThrowsAny<InvalidOperationException>(forbidden);
            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            _ = Record.Exception(() => transaction.Checkpoint());
            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            scope.Dispose();
            Assert.ThrowsAny<InvalidOperationException>(() => transaction.Instantiate(baseline.SourceId));

            using var verification = writerDatabase.CreateTransaction();
            var unchanged = Assert.IsType<C1>(verification.Instantiate(baseline.SourceId));
            Assert.Equal("Committed baseline", unchanged.C1AllorsString);
            Assert.Equal(baseline.SourceVersion, unchanged.Strategy.ObjectVersion);
            Assert.Null(unchanged.C1C2many2one);
            Assert.Empty(unchanged.C1C2many2manies);
            Assert.Single(verification.Extent<C1>().ToArray());
            Assert.Single(verification.Extent<C2>().ToArray());
        }

        protected static void AssertProtectedReadRejectsAccessAfterDisposal(Sql.Database database)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            var scope = database.OpenRead();
            var transaction = scope.Transaction;
            var retained = Assert.IsType<C1>(transaction.Instantiate(baseline.SourceId));
            Assert.Equal("Committed baseline", retained.C1AllorsString);
            transaction.Checkpoint();
            scope.EnsureValid();
            scope.Dispose();
            scope.Dispose();

            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            Assert.ThrowsAny<InvalidOperationException>(() => transaction.Instantiate(baseline.SourceId));
            Assert.ThrowsAny<InvalidOperationException>(() => transaction.Extent<C1>().Count);
            Assert.ThrowsAny<InvalidOperationException>(() => retained.C1AllorsString);
            Assert.ThrowsAny<InvalidOperationException>(() => transaction.Connection.CreateCommand());
            using var fresh = database.OpenRead();
            Assert.Equal("Committed baseline", ((C1)fresh.Transaction.Instantiate(baseline.SourceId)).C1AllorsString);
            fresh.EnsureValid();
        }

        protected static void AssertProtectedReadInitializesServicesAfterSnapshot(
            Sql.Database writerDatabase,
            Func<IDatabaseServices, Sql.Database> createReader)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            var services = new ProtectedReadTestDatabaseServices(transaction =>
            {
                using var writer = writerDatabase.CreateTransaction();
                ((C1)writer.Instantiate(baseline.SourceId)).C1AllorsString = "Changed during service initialization";
                writer.Commit();
                Assert.Equal("Committed baseline", ((C1)transaction.Instantiate(baseline.SourceId)).C1AllorsString);
            });
            var readerDatabase = createReader(services);
            using (var scope = readerDatabase.OpenRead())
            {
                Assert.Equal(1, services.Initializations);
                Assert.Same(services.Transaction, scope.Transaction);
                Assert.Equal("Committed baseline", ((C1)scope.Transaction.Instantiate(baseline.SourceId)).C1AllorsString);
                scope.EnsureValid();
            }

            Assert.Equal(1, services.Disposals);
        }

        protected static void AssertProtectedReadInitializationFailureCleansUp(
            Sql.Database writerDatabase,
            Func<IDatabaseServices, Sql.Database> createReader,
            bool catchViolation)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            var services = new ProtectedReadTestDatabaseServices(transaction =>
            {
                if (catchViolation)
                {
                    Assert.ThrowsAny<InvalidOperationException>(() => transaction.Create<C1>());
                    _ = Record.Exception(() => transaction.Checkpoint());
                }
                else
                {
                    transaction.Create<C1>();
                }
            });
            var readerDatabase = createReader(services);
            Assert.ThrowsAny<InvalidOperationException>(() => readerDatabase.OpenRead());
            Assert.Equal(1, services.Initializations);
            Assert.Equal(1, services.Disposals);
            Assert.NotNull(services.Transaction);
            Assert.ThrowsAny<InvalidOperationException>(() => services.Transaction.Instantiate(baseline.SourceId));
            Assert.ThrowsAny<InvalidOperationException>(() => services.Transaction.Commit());
            using var current = writerDatabase.CreateTransaction();
            Assert.Single(current.Extent<C1>().ToArray());
            Assert.Equal("Committed baseline", ((C1)current.Instantiate(baseline.SourceId)).C1AllorsString);
        }

        protected static void AssertProtectedWriteCapturesChangesAcrossCheckpoints(Sql.Database database)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            var population = database.PopulationLifecycle.RequireReady();
            using var write = database.OpenWrite(population.Incarnation);
            write.AllowWrites();
            var source = (C1)write.Transaction.Instantiate(baseline.SourceId);
            var target = (C2)write.Transaction.Instantiate(baseline.TargetId);
            write.IncludeSubmitted(source.Strategy);
            var noOp = write.Transaction.Create<C1>();
            noOp.C1AllorsString = "unchanged";
            write.Transaction.Checkpoint();
            source.C1C2one2one = target;
            write.Transaction.Checkpoint();
            var createdThenDeleted = write.Transaction.Create<C2>();
            var deletedId = createdThenDeleted.Id;
            createdThenDeleted.Strategy.Delete();
            write.Transaction.Checkpoint();
            source.C1C2one2one = null;
            write.Commit();

            Assert.Equal(Sql.WriteCommitState.Committed, write.CommitState);
            Assert.Equal(4, write.AffectedObjects.Count);
            var sourceEntry = Assert.Single(write.AffectedObjects.Where(v => v.Id == source.Id));
            Assert.True(sourceEntry.Kind.HasFlag(Sql.AffectedObjectKind.Submitted));
            Assert.True(sourceEntry.Kind.HasFlag(Sql.AffectedObjectKind.Role));
            var inverseEntry = Assert.Single(write.AffectedObjects.Where(v => v.Id == target.Id));
            Assert.True(inverseEntry.Kind.HasFlag(Sql.AffectedObjectKind.Association));
            var deletedEntry = Assert.Single(write.AffectedObjects.Where(v => v.Id == deletedId));
            Assert.Equal(database.Context().M.C2, deletedEntry.Class);
            Assert.True(deletedEntry.Kind.HasFlag(Sql.AffectedObjectKind.Created));
            Assert.True(deletedEntry.Kind.HasFlag(Sql.AffectedObjectKind.Deleted));
            Assert.Throws<InvalidOperationException>(() => write.Transaction.Commit());
        }

        protected static void AssertProtectedWriteAdmissionAndDisposal(Sql.Database database)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            var population = database.PopulationLifecycle.RequireReady();
            Assert.Throws<InvalidOperationException>(() => database.OpenWrite(Guid.NewGuid()));
            using (var rejected = database.OpenWrite(population.Incarnation))
            {
                var source = (C1)rejected.Transaction.Instantiate(baseline.SourceId);
                Assert.Throws<InvalidOperationException>(() => source.C1AllorsString = "Rejected");
                Assert.Throws<InvalidOperationException>(() => rejected.AllowWrites());
                Assert.Equal(Sql.WriteCommitState.NotCommitted, rejected.CommitState);
            }

            var write = database.OpenWrite(population.Incarnation);
            write.AllowWrites();
            var changed = (C1)write.Transaction.Instantiate(baseline.SourceId);
            changed.C1AllorsString = "Rolled back";
            write.Dispose();
            Assert.Throws<ObjectDisposedException>(() => changed.C1AllorsString = "After disposal");
            using var check = database.CreateTransaction();
            Assert.Equal("Committed baseline", ((C1)check.Instantiate(baseline.SourceId)).C1AllorsString);
        }

        protected static void AssertProtectedWriteRejectsDirectLifecycleOperations(Sql.Database database, string operation)
        {
            PrepareProtectedReadPopulation(database);
            using var write = database.OpenWrite(database.PopulationLifecycle.RequireReady().Incarnation);
            write.AllowWrites();
            write.Transaction.Create<C1>();
            Action invalid = operation switch
            {
                "commit" => () => write.Transaction.Commit(),
                "rollback" => () => write.Transaction.Rollback(),
                "connection-commit" => () => write.Transaction.Connection.Commit(),
                "connection-rollback" => () => write.Transaction.Connection.Rollback(),
                "connection-command" => () => write.Transaction.Connection.CreateCommand(),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };
            Assert.Throws<InvalidOperationException>(invalid);
            Assert.Throws<InvalidOperationException>(() => write.Commit());
            Assert.Equal(Sql.WriteCommitState.NotCommitted, write.CommitState);
        }

        protected static void AssertProtectedWriteInitializationCannotMutate(
            Sql.Database database, Func<IDatabaseServices, Sql.Database> createWriter, string operation)
        {
            PrepareProtectedReadPopulation(database);
            var population = database.PopulationLifecycle.RequireReady();
            var services = new ProtectedReadTestDatabaseServices(transaction =>
            {
                Action invalid = operation switch
                {
                    "create" => () => transaction.Create<C1>(),
                    "commit" => () => transaction.Commit(),
                    "checkpoint" => () => transaction.Checkpoint(),
                    _ => throw new ArgumentOutOfRangeException(nameof(operation)),
                };
                Assert.Throws<InvalidOperationException>(invalid);
            });
            var writer = createWriter(services);
            Assert.Throws<InvalidOperationException>(() => writer.OpenWrite(population.Incarnation));
            using var check = database.CreateTransaction();
            Assert.Single(check.Extent<C1>().ToArray());
        }

        protected static void AssertProtectedWriteCleanupFailureKeepsCommit(
            Sql.Database database, Func<IDatabaseServices, Sql.Database> createWriter)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            var expected = new InvalidOperationException("Write cleanup failed.");
            var services = new ProtectedReadTestDatabaseServices(_ => { }, () => throw expected);
            var writer = createWriter(services);
            var write = writer.OpenWrite(database.PopulationLifecycle.RequireReady().Incarnation);
            write.AllowWrites();
            ((C1)write.Transaction.Instantiate(baseline.SourceId)).C1AllorsString = "Committed before cleanup";
            write.Commit();
            Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => write.Dispose()));
            Assert.Equal(Sql.WriteCommitState.Committed, write.CommitState);
            using var check = database.CreateTransaction();
            Assert.Equal("Committed before cleanup", ((C1)check.Instantiate(baseline.SourceId)).C1AllorsString);
        }

        protected static (long SourceId, long TargetId, long SourceVersion) PrepareProtectedReadPopulation(Sql.Database database)
        {
            using var maintenance = database.PopulationLifecycle.BeginMaintenance();
            database.Init();
            long sourceId;
            long targetId;
            long sourceVersion;
            using (var setup = database.CreateTransaction())
            {
                var source = setup.Create<C1>();
                source.C1AllorsString = "Committed baseline";
                var target = setup.Create<C2>();
                setup.Commit();
                sourceId = source.Id;
                targetId = target.Id;
                sourceVersion = source.Strategy.ObjectVersion;
            }

            maintenance.Complete();
            return (sourceId, targetId, sourceVersion);
        }

        protected static void AssertProtectedReadCleanupFailureEndsScope(
            Sql.Database writerDatabase,
            Func<IDatabaseServices, Sql.Database> createReader,
            Action assertNativeTransactionEnded)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            var expected = new InvalidOperationException("Expected service cleanup failure.");
            var services = new ProtectedReadTestDatabaseServices(_ => { }, () => throw expected);
            var database = createReader(services);
            var scope = database.OpenRead();
            var transaction = scope.Transaction;
            var retained = Assert.IsType<C1>(transaction.Instantiate(baseline.SourceId));
            Assert.Equal("Committed baseline", retained.C1AllorsString);

            Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => scope.Dispose()));
            Assert.Equal(1, services.Disposals);
            assertNativeTransactionEnded();
            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            Assert.ThrowsAny<InvalidOperationException>(() => retained.C1AllorsString);
            Assert.ThrowsAny<InvalidOperationException>(() => transaction.Instantiate(baseline.SourceId));
            scope.Dispose();
            Assert.Equal(1, services.Disposals);
        }

        protected static void AssertProtectedReadCaughtCleanupMutationEndsScope(
            Sql.Database writerDatabase,
            Func<IDatabaseServices, Sql.Database> createReader,
            Action assertNativeTransactionEnded)
        {
            var baseline = PrepareProtectedReadPopulation(writerDatabase);
            ITransaction retainedTransaction = null;
            var attempted = false;
            var services = new ProtectedReadTestDatabaseServices(
                transaction => retainedTransaction = transaction,
                () =>
                {
                    attempted = true;
                    Assert.ThrowsAny<InvalidOperationException>(() => retainedTransaction.Commit());
                });
            var database = createReader(services);
            var scope = database.OpenRead();
            scope.EnsureValid();

            Assert.ThrowsAny<InvalidOperationException>(() => scope.Dispose());
            Assert.True(attempted);
            Assert.Equal(1, services.Disposals);
            assertNativeTransactionEnded();
            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            Assert.ThrowsAny<InvalidOperationException>(() => retainedTransaction.Instantiate(baseline.SourceId));
            scope.Dispose();
            Assert.Equal(1, services.Disposals);
        }

        protected static void AssertProtectedReadKeepsBinaryValuesIndependent(Sql.Database database)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            long version;
            using (var setup = database.CreateTransaction())
            {
                var source = (C1)setup.Instantiate(baseline.SourceId);
                source.C1AllorsBinary = new byte[] { 1, 2, 3 };
                setup.Commit();
                version = source.Strategy.ObjectVersion;
            }

            using var legacy = database.CreateTransaction();
            var legacySource = (C1)legacy.Instantiate(baseline.SourceId);
            var legacyBytes = legacySource.C1AllorsBinary;
            legacyBytes[0] = 9;
            Assert.Equal(new byte[] { 9, 2, 3 }, legacySource.C1AllorsBinary);
            Assert.Equal(version, legacySource.Strategy.ObjectVersion);

            using (var scope = database.OpenRead())
            {
                var source = (C1)scope.Transaction.Instantiate(baseline.SourceId);
                var readBytes = source.C1AllorsBinary;
                Assert.Equal(new byte[] { 1, 2, 3 }, readBytes);
                readBytes[1] = 8;
                Assert.Equal(new byte[] { 1, 2, 3 }, source.C1AllorsBinary);
                Assert.Equal(baseline.SourceId, source.Id);
                Assert.Equal(version, source.Strategy.ObjectVersion);
                scope.EnsureValid();
            }

            Assert.Equal(new byte[] { 9, 2, 3 }, legacySource.C1AllorsBinary);
            Assert.Equal(version, legacySource.Strategy.ObjectVersion);
        }

        protected static void AssertProtectedReadRejectsRetainedExtentAccess(Sql.Database database)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            var scope = database.OpenRead();
            var transaction = scope.Transaction;
            var retained = (C1)transaction.Instantiate(baseline.SourceId);
            var extent = transaction.Extent<C1>();
            Assert.Equal(1, extent.Count);
            var enumerator = extent.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Same(retained, enumerator.Current);
            scope.Dispose();

            Assert.ThrowsAny<InvalidOperationException>(() => extent.Count);
            Assert.ThrowsAny<InvalidOperationException>(() => extent.Contains(retained));
            Assert.ThrowsAny<InvalidOperationException>(() => enumerator.Current);
            Assert.ThrowsAny<InvalidOperationException>(() => enumerator.MoveNext());
        }

        protected static void AssertProtectedReadRejectsForeignStateReferences(Sql.Database database, bool newObject)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            using var ambient = (Sql.Transaction)database.CreateTransaction();
            var ambientState = ambient.State;
            using var scope = database.OpenRead();
            var transaction = scope.Transaction;
            var objectType = database.Context().M.C1;

            Action forgeReference = newObject
                ? () => ambientState.CreateReferenceForNewObject(objectType, baseline.SourceId, transaction)
                : () => ambientState.GetOrCreateReferenceForExistingObject(objectType, baseline.SourceId, baseline.SourceVersion + 100, transaction);
            Assert.ThrowsAny<InvalidOperationException>(forgeReference);
            Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
            scope.Dispose();

            var source = (C1)ambient.Instantiate(baseline.SourceId);
            Assert.Equal("Committed baseline", source.C1AllorsString);
            Assert.Equal(baseline.SourceVersion, source.Strategy.ObjectVersion);
        }

        protected static void AssertProtectedReadRejectsForeignExtentOperands(Sql.Database database, string operation)
        {
            var baseline = PrepareProtectedReadPopulation(database);
            using var ambient = (Sql.Transaction)database.CreateTransaction();
            var source = (C1)ambient.Instantiate(baseline.SourceId);
            source.C1AllorsString = "Uncommitted ambient value";
            var created = ambient.Create<C1>();

            foreach (var ambientFirst in TrueFalse)
            {
                using var scope = database.OpenRead();
                var transaction = scope.Transaction;
                Allors.Database.Extent ownExtent = transaction.Extent<C1>();
                Allors.Database.Extent ambientExtent = ambient.Extent<C1>();
                var first = ambientFirst ? ambientExtent : ownExtent;
                var second = ambientFirst ? ownExtent : ambientExtent;
                Action combine = operation switch
                {
                    "union" => () => transaction.Union(first, second),
                    "intersect" => () => transaction.Intersect(first, second),
                    "except" => () => transaction.Except(first, second),
                    _ => throw new ArgumentOutOfRangeException(nameof(operation)),
                };

                Assert.ThrowsAny<InvalidOperationException>(combine);
                Assert.ThrowsAny<InvalidOperationException>(() => scope.EnsureValid());
                scope.Dispose();
                Assert.Equal("Uncommitted ambient value", source.C1AllorsString);
                Assert.True(created.Strategy.IsNewInTransaction);
                Assert.Same(created, ambient.Instantiate(created.Id));
            }

            using var verification = database.OpenRead();
            Assert.Equal("Committed baseline", ((C1)verification.Transaction.Instantiate(baseline.SourceId)).C1AllorsString);
            Assert.Null(verification.Transaction.Instantiate(created.Id));
            Allors.Database.Extent ownFirst = verification.Transaction.Extent<C1>();
            Allors.Database.Extent ownSecond = verification.Transaction.Extent<C1>();
            var ownCombination = operation switch
            {
                "union" => verification.Transaction.Union(ownFirst, ownSecond),
                "intersect" => verification.Transaction.Intersect(ownFirst, ownSecond),
                "except" => verification.Transaction.Except(ownFirst, ownSecond),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };
            Assert.Equal(operation == "except" ? 0 : 1, ownCombination.Count);
            verification.EnsureValid();
        }

        protected sealed class ProtectedReadTestDatabaseServices : IDomainDatabaseServices
        {
            private readonly DefaultDomainDatabaseServices inner = new DefaultDomainDatabaseServices();
            private readonly Action<ITransaction> initialize;
            private readonly Action dispose;

            internal ProtectedReadTestDatabaseServices(Action<ITransaction> initialize, Action dispose = null)
            {
                this.initialize = initialize;
                this.dispose = dispose;
            }

            public MetaPopulation M => this.inner.M;

            public ITransaction Transaction { get; private set; }

            public int Initializations { get; private set; }

            public int Disposals { get; private set; }

            public void OnInit(IDatabase database) => this.inner.OnInit(database);

            public ITransactionServices CreateTransactionServices() => new TransactionServices(this);

            public T Get<T>() => this.inner.Get<T>();

            public void Dispose() => this.inner.Dispose();

            private sealed class TransactionServices : IDomainTransactionServices
            {
                private readonly ProtectedReadTestDatabaseServices databaseServices;

                internal TransactionServices(ProtectedReadTestDatabaseServices databaseServices) => this.databaseServices = databaseServices;

                public void OnInit(ITransaction transaction)
                {
                    this.databaseServices.Transaction = transaction;
                    ++this.databaseServices.Initializations;
                    this.databaseServices.initialize(transaction);
                }

                public T Get<T>() => default;

                public void Dispose()
                {
                    ++this.databaseServices.Disposals;
                    this.databaseServices.dispose?.Invoke();
                }
            }
        }

        protected abstract void SwitchDatabase();

        protected abstract IDatabase CreatePopulation();

        protected abstract ITransaction CreateTransaction();

        private IObject[] GetExtent(IComposite objectType) => this.Transaction.Extent(objectType);
    }
}
