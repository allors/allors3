// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace;
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
                c1.Strategy.ScalarRole(this.M.C1.C1AllorsInteger).Set("Not an integer");
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
                c1.Strategy.CompositeRole<IObject>(this.M.C1.C1C1One2One).Set(c2);
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

            c1.Strategy.CompositesRole<C2>(this.M.C1.C1C2Many2Manies).Set(new[] { c2 });

            try
            {
                c1.Strategy.CompositeRole<C2>(this.M.C1.C1C2Many2Manies).Set(c2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
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
                c1.Strategy.CompositesRole<IObject>(this.M.C1.C1C1Many2Manies).Add(c2);
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
                c1.Strategy.CompositesRole<C2>(this.M.C1.C1C2One2One).Add(c2);
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
