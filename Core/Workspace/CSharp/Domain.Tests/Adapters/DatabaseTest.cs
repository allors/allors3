// <copyright file="DatabaseTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters
{
    using System;
    using System.Net.Http;

    using Allors.Workspace;
    using Allors.Workspace.Adapters.Remote;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;

    using Xunit;

    [Collection("Database")]
    public class DatabaseTest : IDisposable
    {
        public const string Url = "http://localhost:5000";

        public const string InitUrl = "/Test/Init";
        public const string SetupUrl = "/Test/Setup?population=full";
        public const string LoginUrl = "/Test/Login";

        public ContextFactory ContextFactory { get; }

        public ClientDatabase Database { get; }

        public Workspace Workspace { get; }

        public M M { get; }

        public DatabaseTest()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(Url),
            };

            this.Database = new ClientDatabase(client);
            var objectFactory = new ObjectFactory(new MetaBuilder().Build(), typeof(User));
            this.Workspace = new Workspace(objectFactory, new WorkspaceState());

            this.ContextFactory = new ContextFactory(this.Database, this.Workspace);

            this.M = this.Workspace.Scope().M;

            this.Init();
        }

        public void Dispose()
        {
        }

        public void Login(string user)
        {
            var uri = new Uri("/TestAuthentication/Token", UriKind.Relative);
            var result = this.Database.Login(uri, user, null).Result;
        }

        private void Init()
        {
            var httpResponseMessage = this.Database.HttpClient.GetAsync(SetupUrl).Result;
            this.Login("administrator");
        }
    }
}
