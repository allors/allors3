// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Session.ToDatabase
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public class LifecycleTests : Test
    {
        [Fact]
        public async void SetRole()
        {
            var session1 = this.Workspace.CreateSession();

            var sessionOrganisation1 = session1.Create<SessionOrganisation>();
            
            var session2 = this.Workspace.CreateSession();

            var sessionOrganisation2 = session2.Instantiate(sessionOrganisation1);

            Assert.Null(sessionOrganisation2);
        }
    }
}
