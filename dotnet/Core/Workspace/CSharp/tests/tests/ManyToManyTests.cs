// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class Many2ManyTests : Test
    {
        protected Many2ManyTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void WorkspaceDatabase_SetRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                }
            };
            #endregion

            var result = await session1.Pull(pulls);

            var session1c1 = result.GetCollection<C1>().First();
            var session1c12 = session1.Create<C1>();

            session1c1.AddC1C1Many2Many(session1c12);

            await session1.Push();

            var session2 = this.Workspace.CreateSession();
            await session2.Pull(pulls);

            var session2c1 = session2.Instantiate<C1>(session1c1);

            Assert.Contains<C1>(session1c12, session1c1.C1C1Many2Manies);
            Assert.Contains<C1>(session1c12, session2c1.C1C1Many2Manies);
        }

        [Fact]
        public async void WorkspaceDatabase_RemoveRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                }
            };
            #endregion

            var result = await session1.Pull(pulls);

            var session1c1 = session1.Create<C1>();
            var session1c12 = result.GetCollection<C1>().First();

            session1c1.AddC1C1Many2Many(session1c12);

            await session1.Push();

            var session2 = this.Workspace.CreateSession();
            await session2.Pull(pulls);

            var session2c1 = session2.Instantiate<C1>(session1c1);

            session1c1.RemoveC1C1Many2Manies();

            Assert.Empty(session1c1.C1C1Many2Manies);
            Assert.Contains<C1>(session1c12, session2c1.C1C1Many2Manies);

            await session1.Push();

            await session2.Pull(pulls);

            Assert.Empty(session1c1.C1C1Many2Manies);
            Assert.Empty(session2c1.C1C1Many2Manies);
        }
    }
}
