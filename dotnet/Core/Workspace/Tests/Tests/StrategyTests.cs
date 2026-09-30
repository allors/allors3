// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class StrategyTests : Test
    {
        protected StrategyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public void SetUnitRoleWrongObjectType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            Assert.NotNull(c1);

            bool hasErrors;

            try
            {
                c1.Strategy.SetUnitRole(this.M.C1.C1AllorsInteger, "Not an integer");
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void SetCompositeRoleWrongObjectType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();
            Assert.NotNull(c1);
            Assert.NotNull(c2);

            bool hasErrors;

            try
            {
                c1.Strategy.SetCompositeRole(this.M.C1.C1C1One2One, c2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void SetCompositeRoleWrongRoleType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();
            Assert.NotNull(c1);
            Assert.NotNull(c2);

            bool hasErrors;

            c1.Strategy.SetCompositesRole(this.M.C1.C1C2Many2Manies, new[] { c2 });

            try
            {
                c1.Strategy.SetCompositeRole(this.M.C1.C1C2Many2Manies, c2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void SetCompositesRoleUnsorted()
        {
            var session1 = this.Workspace.CreateSession();

            var c2a = session1.Create<C2>();
            var c2b = session1.Create<C2>();

            // One of both orders is not the sorted order
            foreach (var role in new[] { new[] { c2a, c2b }, new[] { c2b, c2a } })
            {
                var c1 = session1.Create<C1>();

                c1.Strategy.SetCompositesRole(this.M.C1.C1C2Many2Manies, role);

                Assert.Equal(2, c1.C1C2Many2Manies.Count());
                Assert.Contains(c2a, c1.C1C2Many2Manies);
                Assert.Contains(c2b, c1.C1C2Many2Manies);

                Assert.Contains(c1, c2a.C1sWhereC1C2Many2Many);
                Assert.Contains(c1, c2b.C1sWhereC1C2Many2Many);

                c1.Strategy.RemoveCompositesRole(this.M.C1.C1C2Many2Manies, c2a);
                c1.Strategy.RemoveCompositesRole(this.M.C1.C1C2Many2Manies, c2b);

                Assert.Empty(c1.C1C2Many2Manies);
                Assert.DoesNotContain(c1, c2a.C1sWhereC1C2Many2Many);
                Assert.DoesNotContain(c1, c2b.C1sWhereC1C2Many2Many);
            }
        }

        [Fact]
        public void AddCompositesRoleWrongObjectType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();

            bool hasErrors;
            try
            {
                c1.Strategy.AddCompositesRole(this.M.C1.C1C1Many2Manies, c2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void AddCompositesRoleWrongRoleType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();
            Assert.NotNull(c1);
            Assert.NotNull(c2);

            bool hasErrors;

            try
            {
                c1.Strategy.AddCompositesRole(this.M.C1.C1C2One2One, c2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

    }
}
