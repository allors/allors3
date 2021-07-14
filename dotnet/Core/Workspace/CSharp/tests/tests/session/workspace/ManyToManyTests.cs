// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionWorkspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;

    public abstract class ManyToManyTests : Test
    {
        private ISession session1;
        private SessionC1 c1;
        private WorkspaceC2 c2;

        protected ManyToManyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await this.Login("administrator");

            this.session1 = this.Workspace.CreateSession();

            this.c1 = this.session1.Create<SessionC1>();
            this.c2 = this.session1.Create<WorkspaceC2>();
        }

        [Fact]
        public void SetRole_WithoutPush()
        {
            this.c1.AddSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public async void SetRole_WithPush()
        {
            this.c1.AddSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);

            await this.session1.Push();

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public void RemoveRole_WithoutPush()
        {
            this.c1.AddSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);

            this.c1.RemoveSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.DoesNotContain(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);
        }

        [Fact]
        public async void RemoveRole_WithPush()
        {
            this.c1.AddSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);

            await this.session1.Push();

            Assert.Contains(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);

            this.c1.RemoveSessionC1WorkspaceC2Many2Many(this.c2);

            Assert.DoesNotContain(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);

            await this.session1.Push();

            Assert.DoesNotContain(this.c2, this.c1.SessionC1WorkspaceC2Many2Manies);
        }

    }
}
