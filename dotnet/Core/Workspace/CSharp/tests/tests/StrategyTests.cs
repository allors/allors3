// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
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
        public async void SettingACorrectDataType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1_1 = session1.Create<C1>();
            var c2_1 = session1.Create<C2>();
            Assert.NotNull(c1_1);
            Assert.NotNull(c2_1);

            await this.AsyncDatabaseClient.PushAsync(session1);
            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1_1 });
            var c1_2 = (C1)result.Objects.Values.First();

            Assert.NotNull(c1_2);

            c1_2.Strategy.SetCompositeRole(this.M.C1.C1C2One2One, c2_1);

            Assert.Equal(c2_1, c1_2.C1C2One2One);
            Assert.Equal(c1_2, c2_1.C1WhereC1C2One2One);
        }

        [Fact]
        public void AddingAWrongDataType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();
            Assert.NotNull(c1);
            Assert.NotNull(c2);

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
        public void SettingAWrongUnitType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            Assert.NotNull(c1);

            bool hasErrors;

            try
            {
                c1.Strategy.SetUnitRole(this.M.C1.C1AllorsInteger, "Test");
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void SettingAWrongDataType()
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
        public void AddingAOne2OneUsingTheAddCompositesRoleMethod()
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

        [Fact]
        public void AddingAMany2ManyUsingTheSetCompositeRoleMethod()
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
    }
}
