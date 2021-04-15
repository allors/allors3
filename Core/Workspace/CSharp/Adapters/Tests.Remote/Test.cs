// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System.Net.Http;

    using Allors.Workspace;
    using Allors.Workspace.Adapters.Remote;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Xunit;
    using User = Allors.Workspace.Domain.User;

    [Collection("Database")]
    public class Test
    {
        public RemoteWorkspace Workspace { get; }

        public RemoteDatabase Database => this.Workspace.Database;

        public M M => this.Workspace.Context().M;

        public Test() =>
            this.Workspace = new RemoteWorkspace(
                "Default",

                new MetaBuilder().Build(),
                typeof(User),
                new WorkspaceContext(),
                new HttpClient());
    }
}
