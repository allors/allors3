// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Session
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class ManyToManyTests : Test
    {
        protected ManyToManyTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void SetRole()
        {
            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var c1a_1 = session1.Create<C1>();
            var c1b_1 = session1.Create<C1>();
            var c1c_1 = session1.Create<C1>();
            var c1x_2 = session2.Create<C1>();
            var c1y_2 = session2.Create<C1>();
            var c1z_2 = session2.Create<C1>();

            await this.AsyncDatabaseClient.PushAsync(session2);
            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1x_2 }, new Pull { Object = c1y_2 }, new Pull { Object = c1z_2 });

            var c1x_1 = session1.Instantiate(c1x_2);
            var c1y_1 = session1.Instantiate(c1y_2);
            var c1z_1 = session1.Instantiate(c1z_2);

            c1a_1.AddSessionC1Many2Many(c1b_1);
            c1c_1.AddSessionC1Many2Many(c1x_1);
            c1y_1.AddSessionC1Many2Many(c1z_1);

            Assert.Single(c1a_1.SessionC1Many2Manies);
            Assert.Single(c1c_1.SessionC1Many2Manies);
            Assert.Single(c1y_1.SessionC1Many2Manies);
            Assert.Single(c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1z_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1a_1, c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1c_1, c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1y_1, c1z_1.C1sWhereSessionC1Many2Many);

            await this.AsyncDatabaseClient.PushAsync(session1);

            Assert.Single(c1a_1.SessionC1Many2Manies);
            Assert.Single(c1c_1.SessionC1Many2Manies);
            Assert.Single(c1y_1.SessionC1Many2Manies);
            Assert.Single(c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1z_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1a_1, c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1c_1, c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1y_1, c1z_1.C1sWhereSessionC1Many2Many);
        }
    }
}
