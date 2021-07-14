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

        #region WorkspaceWorkspace

        [Fact]
        public async void WorkspaceWorkspace_SetRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<WorkspaceC2>();

            c1.AddWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceWorkspace_SetRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<WorkspaceC2>();

            c1.AddWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);

            await session1.Push();

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceWorkspace_RemoveRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<WorkspaceC2>();

            c1.AddWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);

            c1.RemoveWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceWorkspace_RemoveRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<WorkspaceC2>();

            c1.AddWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);

            await session1.Push();

            Assert.Contains(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);

            c1.RemoveWorkspaceC1WorkspaceC2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);

            await session1.Push();

            Assert.DoesNotContain(c2, c1.WorkspaceC1WorkspaceC2Many2Manies);
        }

        #endregion

        #region WorkspaceDatabase

        [Fact]
        public async void WorkspaceDatabase_SetRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<C2>();

            c1.AddWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceDatabase_SetRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<C2>();

            await session1.Push();

            c1.AddWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);

            await session1.Push();

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceDatabase_RemoveRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<C2>();

            c1.AddWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);

            c1.RemoveWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.WorkspaceC1DatabaseC2Many2Manies);
        }

        [Fact]
        public async void WorkspaceDatabase_RemoveRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<C2>();

            await session1.Push();

            c1.AddWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);

            await session1.Push();

            Assert.Contains(c2, c1.WorkspaceC1DatabaseC2Many2Manies);

            c1.RemoveWorkspaceC1DatabaseC2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.WorkspaceC1DatabaseC2Many2Manies);

            await session1.Push();

            Assert.DoesNotContain(c2, c1.WorkspaceC1DatabaseC2Many2Manies);
        }

        #endregion

        #region DatabaseDatabase

        [Fact]
        public async void DatabaseDatabase_SetRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();

            c1.AddC1C2Many2Many(c2);

            Assert.Contains(c2, c1.C1C2Many2Manies);
        }

        [Fact]
        public async void DatabaseDatabase_SetRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();

            await session1.Push();

            #region pulls

            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                },
                new Pull
                {
                    Extent = new Filter(this.M.C2)
                }
            };

            #endregion

            await session1.Pull(pulls);

            c1.AddC1C2Many2Many(c2);

            Assert.Contains(c2, c1.C1C2Many2Manies);

            await session1.Push();

            // TODO:
            await session1.Pull(pulls);

            Assert.Contains(c2, c1.C1C2Many2Manies);
        }

        [Fact]
        public async void DatabaseDatabase_RemoveRole_WithoutPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();

            c1.AddC1C2Many2Many(c2);

            Assert.Contains(c2, c1.C1C2Many2Manies);

            c1.RemoveC1C2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.C1C2Many2Manies);
        }

        [Fact]
        public async void DatabaseDatabase_RemoveRole_WithPush()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            var c2 = session1.Create<C2>();

            await session1.Push();

            #region pulls

            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                },
                new Pull
                {
                    Extent = new Filter(this.M.C2)
                }
            };

            #endregion

            await session1.Pull(pulls);

            c1.AddC1C2Many2Many(c2);

            Assert.Contains(c2, c1.C1C2Many2Manies);

            await session1.Push();

            // TODO:
            await session1.Pull(pulls);

            Assert.Contains(c2, c1.C1C2Many2Manies);

            c1.RemoveC1C2Many2Many(c2);

            Assert.DoesNotContain(c2, c1.C1C2Many2Manies);

            await session1.Push();

            Assert.DoesNotContain(c2, c1.C1C2Many2Manies);
        }

        #endregion
    }
}
