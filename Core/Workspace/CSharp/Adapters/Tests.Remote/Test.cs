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

    [Collection("Database")]
    public class Test
    {
        public Workspace Workspace { get; }

        public Database Database => this.Workspace.Database;

        public M M => this.Workspace.State().M;

        public Test() =>
            this.Workspace = new Workspace(
                new MetaBuilder().Build(),
                typeof(User),
                new WorkspaceState(),
                new HttpClient());
    }
}
