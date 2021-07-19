// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginWorkspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using System;
    using Allors.Workspace.Data;

    public abstract class WorkspaceStrategyTests : Test
    {
        protected WorkspaceStrategyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public void AddingAWrongDataType()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            var c2 = session1.Create<WorkspaceC2>();
            Assert.NotNull(c1);
            Assert.NotNull(c2);

            bool hasErrors;

            try
            {
                c1.Strategy.AddCompositesRole(this.M.WorkspaceC1.WorkspaceC1WorkspaceC1One2One, c2);
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
