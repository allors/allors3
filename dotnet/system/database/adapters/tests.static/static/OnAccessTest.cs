// <copyright file="One2OneTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using Tracing;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;
    using DateTime = System.DateTime;

    public abstract class OnAccessTest : IDisposable
    {
        protected abstract IProfile Profile { get; }

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        protected IOnAccess OnAccess => (IOnAccess)this.Transaction;

        public abstract void Dispose();

        [Fact]
        public virtual void UnitRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    var list = new List<(IStrategy strategy, IRoleType roleType)>();

                    void OnAccessUnitRole(IStrategy strategy, IRoleType roleType) => list.Add((strategy, roleType));

                    {
                        this.OnAccess.OnAccessUnitRole = OnAccessUnitRole;

                        var c1 = C1.Create(this.Transaction);

                        c1.C1AllorsBinary = new byte[] { 0 };
                        c1.I1AllorsBinary = new byte[] { 1, 2 };

                        c1.C1AllorsBoolean = true;
                        c1.I1AllorsBoolean = false;

                        c1.C1AllorsDateTime = DateTime.UtcNow;
                        c1.I1AllorsDateTime = DateTime.UtcNow.AddDays(1);

                        c1.C1AllorsDecimal = 0.1m;
                        c1.I1AllorsDecimal = 1.0m;

                        c1.C1AllorsDouble = 0.1f;
                        c1.I1AllorsDouble = 1.0f;

                        c1.C1AllorsInteger = 0;
                        c1.I1AllorsInteger = 1;

                        c1.C1AllorsString = "a";
                        c1.I1AllorsString = "b";

                        c1.C1AllorsUnique = new Guid("9A6A32E8-87FA-4423-9885-CF524662D5B8");
                        c1.I1AllorsUnique = Guid.Empty;

                        mark();

                        var c1AllorsBinary = c1.C1AllorsBinary;
                        var existI1AllorsBinary = c1.ExistI1AllorsBinary;
                        var s1AllorsBinary = c1.S1AllorsBinary;

                        var c1AllorsBoolean = c1.C1AllorsBoolean;
                        var existI1AllorsBoolean = c1.ExistI1AllorsBoolean;
                        var s1AllorsBoolean = c1.S1AllorsBoolean;

                        var c1AllorsDateTime = c1.C1AllorsDateTime;
                        var existI1AllorsDateTime = c1.ExistI1AllorsDateTime;
                        var s1AllorsDateTime = c1.S1AllorsDateTime;

                        var c1AllorsDecimal = c1.C1AllorsDecimal;
                        var existI1AllorsDecimal = c1.ExistI1AllorsDecimal;
                        var s1AllorsDecimal = c1.S1AllorsDecimal;

                        var c1AllorsDouble = c1.C1AllorsDouble;
                        var existI1AllorsDouble = c1.ExistI1AllorsDouble;
                        var s1AllorsDouble = c1.S1AllorsDouble;

                        var c1AllorsInteger = c1.C1AllorsInteger;
                        var existI1AllorsInteger = c1.ExistI1AllorsInteger;
                        var s1AllorsInteger = c1.S1AllorsInteger;

                        var c1AllorsString = c1.C1AllorsString;
                        var existI1AllorsString = c1.ExistI1AllorsString;
                        var s1AllorsString = c1.S1AllorsString;

                        var c1AllorsUnique = c1.C1AllorsUnique;
                        var existI1AllorsUnique = c1.ExistI1AllorsUnique;
                        var s1AllorsUnique = c1.S1AllorsUnique;

                        Assert.Equal(24, list.Count);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsBinary), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsBinary), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsBinary), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsBoolean), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsBoolean), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsBoolean), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsDateTime), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsDateTime), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsDateTime), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsDecimal), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsDecimal), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsDecimal), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsDouble), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsDouble), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsDouble), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsInteger), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsInteger), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsInteger), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsString), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsString), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsString), list);

                        Assert.Contains((c1.Strategy, m.C1.C1AllorsUnique), list);
                        Assert.Contains((c1.Strategy, m.C1.I1AllorsUnique), list);
                        Assert.Contains((c1.Strategy, m.C1.S1AllorsUnique), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void RoleOne2One()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IRoleType roleType)>();

                        void OnAccessCompositeRole(IStrategy strategy, IRoleType roleType) => list.Add((strategy, roleType));

                        this.OnAccess.OnAccessCompositeRole = OnAccessCompositeRole;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);

                        c1a.C1C2one2one = c2a;
                        c1b.C1C2one2one = c2b;

                        mark();

                        var c1aC1C2one2one = c1a.C1C2one2one;
                        var c1bExistC1C2one2one = c1b.ExistC1C2one2one;
                        var c1cC1C2one2one = c1c.C1C2one2one;
                        var c1dExistC1C2one2one = c1d.ExistC1C2one2one;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c1a.Strategy, m.C1.C1C2one2one), list);
                        Assert.Contains((c1b.Strategy, m.C1.C1C2one2one), list);
                        Assert.Contains((c1c.Strategy, m.C1.C1C2one2one), list);
                        Assert.Contains((c1d.Strategy, m.C1.C1C2one2one), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void RoleMany2One()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IRoleType roleType)>();

                        void OnAccessCompositeRole(IStrategy strategy, IRoleType roleType) => list.Add((strategy, roleType));

                        this.OnAccess.OnAccessCompositeRole = OnAccessCompositeRole;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);

                        c1a.C1C2many2one = c2a;
                        c1b.C1C2many2one = c2b;

                        mark();

                        var c1aC1C2many2one = c1a.C1C2many2one;
                        var c1bExistC1C2many2one = c1b.ExistC1C2many2one;
                        var c1cC1C2many2one = c1c.C1C2many2one;
                        var c1dExistC1C2many2one = c1d.ExistC1C2many2one;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c1a.Strategy, m.C1.C1C2many2one), list);
                        Assert.Contains((c1b.Strategy, m.C1.C1C2many2one), list);
                        Assert.Contains((c1c.Strategy, m.C1.C1C2many2one), list);
                        Assert.Contains((c1d.Strategy, m.C1.C1C2many2one), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void RoleOne2Many()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IRoleType roleType)>();

                        void OnAccessCompositesRole(IStrategy strategy, IRoleType roleType) => list.Add((strategy, roleType));

                        this.OnAccess.OnAccessCompositesRole = OnAccessCompositesRole;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1b.AddC1C2one2many(c2b);
                        c1b.AddC1C2one2many(c2c);
                        c1b.AddC1C2one2many(c2d);

                        mark();

                        var c1aC1C2one2many = c1a.C1C2one2manies;
                        var c1bExistC1C2one2one = c1b.ExistC1C2one2manies;
                        var c1cC1C2one2one = c1c.C1C2one2manies;
                        var c1dExistC1C2one2one = c1d.ExistC1C2one2manies;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c1a.Strategy, m.C1.C1C2one2manies), list);
                        Assert.Contains((c1b.Strategy, m.C1.C1C2one2manies), list);
                        Assert.Contains((c1c.Strategy, m.C1.C1C2one2manies), list);
                        Assert.Contains((c1d.Strategy, m.C1.C1C2one2manies), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void RoleMany2Many()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IRoleType roleType)>();

                        void OnAccessCompositesRole(IStrategy strategy, IRoleType roleType) => list.Add((strategy, roleType));

                        this.OnAccess.OnAccessCompositesRole = OnAccessCompositesRole;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1b.AddC1C2many2many(c2b);
                        c1b.AddC1C2many2many(c2c);
                        c1b.AddC1C2many2many(c2d);

                        mark();

                        var c1aC1C2many2many = c1a.C1C2many2manies;
                        var c1bExistC1C2many2one = c1b.ExistC1C2many2manies;
                        var c1cC1C2many2one = c1c.C1C2many2manies;
                        var c1dExistC1C2many2one = c1d.ExistC1C2many2manies;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c1a.Strategy, m.C1.C1C2many2manies), list);
                        Assert.Contains((c1b.Strategy, m.C1.C1C2many2manies), list);
                        Assert.Contains((c1c.Strategy, m.C1.C1C2many2manies), list);
                        Assert.Contains((c1d.Strategy, m.C1.C1C2many2manies), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void AssociationOne2One()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IAssociationType AssociationType)>();

                        void OnAccessCompositeAssociation(IStrategy strategy, IAssociationType associationType) => list.Add((strategy, associationType));

                        this.OnAccess.OnAccessCompositeAssociation = OnAccessCompositeAssociation;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1a.C1C2one2one = c2a;
                        c1b.C1C2one2one = c2b;

                        mark();

                        var c2aC1WhereC1C2one2one = c2a.C1WhereC1C2one2one;
                        var c2bExistC1WhereC1C2one2one = c2b.ExistC1WhereC1C2one2one;
                        var c2cC1WhereC1C2one2one = c2c.C1WhereC1C2one2one;
                        var c2dExistC1WhereC1C2one2one = c2d.ExistC1WhereC1C2one2one;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c2a.Strategy, m.C2.C1WhereC1C2one2one), list);
                        Assert.Contains((c2b.Strategy, m.C2.C1WhereC1C2one2one), list);
                        Assert.Contains((c2c.Strategy, m.C2.C1WhereC1C2one2one), list);
                        Assert.Contains((c2d.Strategy, m.C2.C1WhereC1C2one2one), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void AssociationOne2Many()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IAssociationType AssociationType)>();

                        void OnAccessCompositeAssociation(IStrategy strategy, IAssociationType associationType) => list.Add((strategy, associationType));

                        this.OnAccess.OnAccessCompositeAssociation = OnAccessCompositeAssociation;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1a.AddC1C2one2many(c2a);
                        c1b.AddC1C2one2many(c2b);
                        c1c.AddC1C2one2many(c2b);

                        mark();

                        var c2aC1WhereC1C2one2many = c2a.C1WhereC1C2one2many;
                        var c2bExistC1WhereC1C2one2many = c2b.ExistC1WhereC1C2one2many;
                        var c2cC1WhereC1C2one2many = c2c.C1WhereC1C2one2many;
                        var c2dExistC1WhereC1C2one2many = c2d.ExistC1WhereC1C2one2many;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c2a.Strategy, m.C2.C1WhereC1C2one2many), list);
                        Assert.Contains((c2b.Strategy, m.C2.C1WhereC1C2one2many), list);
                        Assert.Contains((c2c.Strategy, m.C2.C1WhereC1C2one2many), list);
                        Assert.Contains((c2d.Strategy, m.C2.C1WhereC1C2one2many), list);
                    }
                }
            }
        }
        
        [Fact]
        public virtual void AssociationMany2One()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IAssociationType AssociationType)>();

                        void OnAccessCompositesAssociation(IStrategy strategy, IAssociationType associationType) => list.Add((strategy, associationType));

                        this.OnAccess.OnAccessCompositesAssociation = OnAccessCompositesAssociation;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1a.C1C2many2one = c2a;
                        c1b.C1C2many2one = c2b;
                        c1c.C1C2many2one = c2b;

                        mark();

                        var c2aC1WhereC1C2many2one = c2a.C1sWhereC1C2many2one;
                        var c2bExistC1WhereC1C2many2one = c2b.ExistC1sWhereC1C2many2one;
                        var c2cC1WhereC1C2many2one = c2c.C1sWhereC1C2many2one;
                        var c2dExistC1WhereC1C2many2one = c2d.ExistC1sWhereC1C2many2one;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c2a.Strategy, m.C2.C1sWhereC1C2many2one), list);
                        Assert.Contains((c2b.Strategy, m.C2.C1sWhereC1C2many2one), list);
                        Assert.Contains((c2c.Strategy, m.C2.C1sWhereC1C2many2one), list);
                        Assert.Contains((c2d.Strategy, m.C2.C1sWhereC1C2many2one), list);
                    }
                }
            }
        }

        [Fact]
        public virtual void AssociationMany2Many()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                foreach (var mark in this.Markers)
                {
                    for (var run = 0; run < Settings.NumberOfRuns; run++)
                    {
                        var list = new List<(IStrategy strategy, IAssociationType AssociationType)>();

                        void OnAccessCompositesAssociation(IStrategy strategy, IAssociationType associationType) => list.Add((strategy, associationType));

                        this.OnAccess.OnAccessCompositesAssociation = OnAccessCompositesAssociation;

                        var c1a = C1.Create(this.Transaction);
                        var c1b = C1.Create(this.Transaction);
                        var c1c = C1.Create(this.Transaction);
                        var c1d = C1.Create(this.Transaction);

                        var c2a = C2.Create(this.Transaction);
                        var c2b = C2.Create(this.Transaction);
                        var c2c = C2.Create(this.Transaction);
                        var c2d = C2.Create(this.Transaction);

                        c1a.AddC1C2many2many(c2a);
                        c1b.AddC1C2many2many(c2a);
                        c1b.AddC1C2many2many(c2b);
                        c1c.AddC1C2many2many(c2b);

                        mark();

                        var c2aC1WhereC1C2many2many = c2a.C1sWhereC1C2many2many;
                        var c2bExistC1WhereC1C2many2many = c2b.ExistC1sWhereC1C2many2many;
                        var c2cC1WhereC1C2many2many = c2c.C1sWhereC1C2many2many;
                        var c2dExistC1WhereC1C2many2many = c2d.ExistC1sWhereC1C2many2many;

                        Assert.Equal(4, list.Count);

                        Assert.Contains((c2a.Strategy, m.C2.C1sWhereC1C2many2many), list);
                        Assert.Contains((c2b.Strategy, m.C2.C1sWhereC1C2many2many), list);
                        Assert.Contains((c2c.Strategy, m.C2.C1sWhereC1C2many2many), list);
                        Assert.Contains((c2d.Strategy, m.C2.C1sWhereC1C2many2many), list);
                    }
                }
            }
        }

    }
}
