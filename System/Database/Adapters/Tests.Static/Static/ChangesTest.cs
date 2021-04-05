// <copyright file="ChangesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ExtentTest type.</summary>

namespace Allors.Database.Adapters
{
    using System;
    using System.Linq;

    using Allors;
    using Domain;
    using Xunit;

    public abstract class ChangesTest : IDisposable
    {
        protected abstract IProfile Profile { get; }

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void UnitRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var a = (C1)this.Transaction.Create(m.C1);
                var c = this.Transaction.Create(m.C3);
                this.Transaction.Commit();

                a = (C1)this.Transaction.Instantiate(a);
                var b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c);

                a.RemoveC1AllorsString();
                b.RemoveC2AllorsString();

                var changeSet = this.Transaction.Checkpoint();

                var associations = changeSet.Associations;
                var roles = changeSet.Roles;

                Assert.Empty(associations);
                Assert.Empty(roles);

                a.C1AllorsString = "a changed";
                b.C2AllorsString = "b changed";

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(2, associations.Count);
                Assert.Contains(a.Id, associations.ToArray());
                Assert.Contains(b.Id, associations.ToArray());

                Assert.Equal("a changed", a.C1AllorsString);
                Assert.Equal("b changed", b.C2AllorsString);

                Assert.Single(changeSet.GetRoleTypes(a.Id));
                Assert.Equal(m.C1.C1AllorsString, changeSet.GetRoleTypes(a.Id).First());

                Assert.Single(changeSet.GetRoleTypes(b.Id));
                Assert.Equal(m.C2.C2AllorsString, changeSet.GetRoleTypes(b.Id).First());

                Assert.True(associations.Contains(a.Id));
                Assert.True(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));
                Assert.False(roles.Contains(c.Id));

                a.C1AllorsString = "a changed";
                b.C2AllorsString = "b changed";

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Empty(associations);
                Assert.Empty(roles);

                a.C1AllorsString = "a changed again";
                b.C2AllorsString = "b changed again";

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(2, associations.Count);
                Assert.True(associations.Contains(a.Id));
                Assert.True(associations.Contains(a.Id));

                Assert.Single(changeSet.GetRoleTypes(a.Id));
                Assert.Equal(m.C1.C1AllorsString, changeSet.GetRoleTypes(a.Id).First());

                Assert.Single(changeSet.GetRoleTypes(b.Id));
                Assert.Equal(m.C2.C2AllorsString, changeSet.GetRoleTypes(b.Id).First());

                Assert.True(associations.Contains(a.Id));
                Assert.True(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));
                Assert.False(roles.Contains(c.Id));

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(0, associations.Count);
                Assert.Empty(changeSet.GetRoleTypes(a.Id));
                Assert.Empty(changeSet.GetRoleTypes(b.Id));

                Assert.False(associations.Contains(a.Id));
                Assert.False(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));
                Assert.False(roles.Contains(c.Id));

                a.RemoveC1AllorsString();
                b.RemoveC2AllorsString();

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(2, associations.Count);
                Assert.True(associations.Contains(a.Id));
                Assert.True(associations.Contains(a.Id));

                Assert.Single(changeSet.GetRoleTypes(a.Id));
                Assert.Equal(m.C1.C1AllorsString, changeSet.GetRoleTypes(a.Id).First());

                Assert.Single(changeSet.GetRoleTypes(b.Id));
                Assert.Equal(m.C2.C2AllorsString, changeSet.GetRoleTypes(b.Id).First());

                Assert.True(associations.Contains(a.Id));
                Assert.True(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));
                Assert.False(roles.Contains(c.Id));

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(0, associations.Count);
                Assert.Empty(changeSet.GetRoleTypes(a.Id));
                Assert.Empty(changeSet.GetRoleTypes(b.Id));

                Assert.False(associations.Contains(a.Id));
                Assert.False(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));

                this.Transaction.Rollback();

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(0, associations.Count);
                Assert.Empty(changeSet.GetRoleTypes(a.Id));
                Assert.Empty(changeSet.GetRoleTypes(b.Id));

                Assert.False(associations.Contains(a.Id));
                Assert.False(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));

                a.C1AllorsString = "a changed";

                this.Transaction.Commit();

                changeSet = this.Transaction.Checkpoint();

                associations = changeSet.Associations;
                roles = changeSet.Roles;

                Assert.Equal(0, associations.Count);
                Assert.Empty(changeSet.GetRoleTypes(a.Id));
                Assert.Empty(changeSet.GetRoleTypes(b.Id));

                Assert.False(associations.Contains(a.Id));
                Assert.False(associations.Contains(b.Id));
                Assert.False(associations.Contains(c.Id));

                Assert.False(roles.Contains(a.Id));
                Assert.False(roles.Contains(b.Id));
            }
        }

        [Fact]
        public void One2OneRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1a = (C1)this.Transaction.Create(m.C1);
                var c1b = (C1)this.Transaction.Create(m.C1);
                var c2a = (C2)this.Transaction.Create(m.C2);

                this.Transaction.Commit();

                c1a = (C1)this.Transaction.Instantiate(c1a);
                var c2b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c2a);

                var changes = this.Transaction.Checkpoint();

                c1a.C1C2one2one = null;

                var associations = changes.Associations.ToArray();
                var roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2one2one();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2one2one = c2b;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                c1a.C1C2one2one = c2b;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2one2one = c2a;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Equal(2, roles.Length);
                Assert.Contains(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2one2one();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Strategy.ObjectId, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.C1C2one2one = c2a;

                this.Transaction.Rollback();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.C1C2one2one = c2a;

                this.Transaction.Commit();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1b.C1C2one2one = c2a;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Equal(2, associations.Length);
                Assert.Single(roles);
                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Single(changes.GetRoleTypes(c1b.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));
            }
        }

        [Fact]
        public void Many2OneRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1a = (C1)this.Transaction.Create(m.C1);
                var c1b = (C1)this.Transaction.Create(m.C1);
                var c2a = (C2)this.Transaction.Create(m.C2);

                this.Transaction.Commit();

                c1a = (C1)this.Transaction.Instantiate(c1a);
                var c2b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c2a);

                var changes = this.Transaction.Checkpoint();

                c1a.C1C2many2one = null;

                var associations = changes.Associations.ToArray();
                var roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2many2one();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2many2one = c2b;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                c1a.C1C2many2one = c2b;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2many2one = c2a;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Equal(2, roles.Length);
                Assert.Contains(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2many2one();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Strategy.ObjectId, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2one, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.C1C2many2one = c2a;

                this.Transaction.Rollback();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.C1C2many2one = c2a;

                this.Transaction.Commit();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1b.C1C2many2one = c2a;

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Single(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Single(changes.GetRoleTypes(c1b.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));
            }
        }

        [Fact]
        public void One2ManyRoles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1a = (C1)this.Transaction.Create(m.C1);
                var c1b = (C1)this.Transaction.Create(m.C1);
                var c2a = (C2)this.Transaction.Create(m.C2);

                this.Transaction.Commit();

                c1a = (C1)this.Transaction.Instantiate(c1a);
                var c2b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c2a);

                c1a.C1C2one2manies = null;

                var changes = this.Transaction.Checkpoint();

                var associations = changes.Associations.ToArray();
                var roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2one2manies();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2one2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.AddC1C2one2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                c1a.AddC1C2one2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2one2manies = new[] { c2b };

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.AddC1C2one2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2one2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2one2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2one2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.AddC1C2one2many(c2a);

                this.Transaction.Rollback();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.AddC1C2one2many(c2a);

                this.Transaction.Commit();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1b.AddC1C2one2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Equal(2, associations.Length);
                Assert.Single(roles);
                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Single(changes.GetRoleTypes(c1b.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));
            }
        }

        [Fact]
        public void Many2ManyRoles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1a = (C1)this.Transaction.Create(m.C1);
                var c1b = (C1)this.Transaction.Create(m.C1);
                var c2a = (C2)this.Transaction.Create(m.C2);

                this.Transaction.Commit();

                c1a = (C1)this.Transaction.Instantiate(c1a);
                var c2b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c2a);

                c1a.C1C2many2manies = null;

                var changes = this.Transaction.Checkpoint();

                var associations = changes.Associations.ToArray();
                var roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2many2manies();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.RemoveC1C2many2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.AddC1C2many2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                c1a.AddC1C2many2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.C1C2many2manies = new[] { c2b };

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                c1a.AddC1C2many2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2many2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2a.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.DoesNotContain(c2b.Id, roles);
                Assert.Contains(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.RemoveC1C2many2many(c2b);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Contains(c1a.Id, associations);

                Assert.Single(roles);
                Assert.Contains(c2b.Id, roles);

                Assert.Single(changes.GetRoleTypes(c1a.Id));
                Assert.Equal(m.C1.C1C2many2manies, changes.GetRoleTypes(c1a.Id).First());

                Assert.Contains(c1a.Id, associations);
                Assert.DoesNotContain(c2b.Id, associations);
                Assert.DoesNotContain(c2a.Id, associations);

                Assert.DoesNotContain(c1a.Id, roles);
                Assert.Contains(c2b.Id, roles);
                Assert.DoesNotContain(c2a.Id, roles);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.AddC1C2many2many(c2a);

                this.Transaction.Rollback();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1a.AddC1C2many2many(c2a);

                this.Transaction.Commit();

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Empty(associations);
                Assert.Empty(roles);

                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));

                c1b.AddC1C2many2many(c2a);

                changes = this.Transaction.Checkpoint();

                associations = changes.Associations.ToArray();
                roles = changes.Roles.ToArray();

                Assert.Single(associations);
                Assert.Single(roles);
                Assert.Empty(changes.GetRoleTypes(c1a.Id));
                Assert.Single(changes.GetRoleTypes(c1b.Id));
                Assert.Empty(changes.GetRoleTypes(c2b.Id));
                Assert.Empty(changes.GetRoleTypes(c2a.Id));
            }
        }

        [Fact]
        public void Delete()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var a = (C1)this.Transaction.Create(m.C1);
                var c = this.Transaction.Create(m.C3);
                this.Transaction.Commit();

                a = (C1)this.Transaction.Instantiate(a);
                var b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c);

                a.Strategy.Delete();
                b.Strategy.Delete();

                var changes = this.Transaction.Checkpoint();

                Assert.Equal(2, changes.Deleted.Count());
                Assert.Contains(a.Strategy, changes.Deleted.ToArray());
                Assert.Contains(b.Strategy, changes.Deleted.ToArray());

                this.Transaction.Rollback();

                changes = this.Transaction.Checkpoint();

                Assert.Empty(changes.Deleted);

                a.Strategy.Delete();

                this.Transaction.Commit();

                changes = this.Transaction.Checkpoint();

                Assert.Empty(changes.Deleted);
            }
        }

        [Fact]
        public void Create()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var a = (C1)this.Transaction.Create(m.C1);
                var c = this.Transaction.Create(m.C3);
                this.Transaction.Commit();

                a = (C1)this.Transaction.Instantiate(a);
                var b = C2.Create(this.Transaction);
                this.Transaction.Instantiate(c);

                var changes = this.Transaction.Checkpoint();

                Assert.Single(changes.Created);
                Assert.Contains(b.Strategy, changes.Created.ToArray());

                this.Transaction.Rollback();

                changes = this.Transaction.Checkpoint();

                Assert.Empty(changes.Created);

                b = C2.Create(this.Transaction);

                this.Transaction.Commit();

                changes = this.Transaction.Checkpoint();

                Assert.Empty(changes.Created);
            }
        }
    }
}
