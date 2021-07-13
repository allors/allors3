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

    public abstract class One2OneTests : Test
    {
        protected One2OneTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void WorkspaceDatabase_SetRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new Pull
            {
                Extent = new Filter(this.M.C1)
            };
            #endregion

            var result = await session1.Pull(pulls);

            var workspaceC11 = session1.Create<C1>();
            var databaseC11 = result.GetCollection<C1>().First();

            workspaceC11.C1C1One2One = databaseC11;

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            await session2.Pull(pulls);

            var workspaceC12 = session2.Instantiate(workspaceC11);
            var databaseC12 = session2.Instantiate(databaseC11);

            Assert.Equal(databaseC11, workspaceC11.C1C1One2One);
            Assert.Equal(databaseC12, workspaceC12.C1C1One2One);
        }

        [Fact]
        public async void WorkspaceDatabase_RemoveRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new Pull
            {
                Extent = new Filter(this.M.C1)
            };
            #endregion

            var result = await session1.Pull(pulls);

            var workspaceC11 = session1.Create<C1>();
            var databaseC11 = result.GetCollection<C1>().First();

            workspaceC11.C1C1One2One = databaseC11;

            await session1.Push();
            await session1.Pull(pulls);

            var session2 = this.Workspace.CreateSession();

            var workspaceC12 = session2.Instantiate(workspaceC11);

            workspaceC11.RemoveC1C1One2One();

            Assert.Null(workspaceC11.C1C1One2One);
            Assert.NotNull(workspaceC12.C1C1One2One);

            await session1.Push();

            Assert.Null(workspaceC11.C1C1One2One);
            Assert.NotNull(workspaceC12.C1C1One2One);

            await session2.Pull(pulls);

            Assert.Null(workspaceC11.C1C1One2One);
            Assert.Null(workspaceC12.C1C1One2One);

            //Assert.Null(workspaceC11.C1C1One2One);
            //Assert.Null(workspaceC12.C1C1One2One);
        }

        [Fact]
        public async void WorkspaceWorkspace_SetRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new Pull
            {
                Extent = new Filter(this.M.C1)
            };
            #endregion

            var workspaceC11 = session1.Create<C1>();
            var workspaceC12 = session1.Create<C1>();

            workspaceC11.C1C1One2One = workspaceC12;

            await session1.Push();
            await session1.Pull(pulls);

            var session2 = this.Workspace.CreateSession();

            var workspaceC13 = session2.Instantiate(workspaceC11);
            var workspaceC14 = session2.Instantiate(workspaceC12);


            Assert.Equal(workspaceC12, workspaceC11.C1C1One2One);
            Assert.Equal(workspaceC14, workspaceC13.C1C1One2One);
        }

        [Fact]
        public async void WorkspaceWorkspace_RemoveRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new Pull
            {
                Extent = new Filter(this.M.C1)
            };
            #endregion

            var workspaceC11 = session1.Create<C1>();
            var workspaceC12 = session1.Create<C1>();

            workspaceC11.C1C1One2One = workspaceC12;

            await session1.Push();
            await session1.Pull(pulls);

            var session2 = this.Workspace.CreateSession();

            var workspaceC13 = session2.Instantiate(workspaceC11);
            var workspaceC14 = session2.Instantiate(workspaceC12);

            Assert.NotNull(workspaceC13.C1C1One2One);

            workspaceC11.RemoveC1C1One2One();

            Assert.Null(workspaceC11.C1C1One2One);
            Assert.NotNull(workspaceC13.C1C1One2One);

            await session1.Push();

            Assert.Null(workspaceC11.C1C1One2One);
            Assert.NotNull(workspaceC13.C1C1One2One);

            await session2.Pull(new Pull
            {
                Object = workspaceC11
            });
            
            Assert.Null(workspaceC11.C1C1One2One);
            Assert.Null(workspaceC13.C1C1One2One);
        }
    }
}
