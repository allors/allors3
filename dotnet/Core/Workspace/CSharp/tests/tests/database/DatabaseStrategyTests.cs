// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginDatabase
{
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class DatabaseStrategyTests : Test
    {
        protected DatabaseStrategyTests(Fixture fixture) : base(fixture)
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

            var c1 = session1.Create<C1>();
            Assert.NotNull(c1);

            c1.Strategy.AddCompositesRole(this.M.C1.C1C1Many2One, session1.Create<C2>());
            Assert.Null(c1.C1C1Many2One);
        }
    }
}
