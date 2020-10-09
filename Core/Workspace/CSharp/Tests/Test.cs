// <copyright file="Test.cs" company="Allors bvba">
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
    using InternalWorkspace = Allors.Workspace.Adapters.Remote.InternalWorkspace;

    [Collection("Database")]
    public class Test : IDisposable
    {
        public const string Url = "http://localhost:5000";

        public const string InitUrl = "/Test/Init";
        public const string SetupUrl = "/Test/Setup?population=full";
        public const string LoginUrl = "/Test/Login";

        public ContextFactory ContextFactory { get; }

        public ClientDatabase Database { get; }

        public InternalWorkspace InternalWorkspace { get; }

        public M M { get; }

        public Test()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(Url),
            };

            this.Database = new ClientDatabase(client);
            var objectFactory = new ObjectFactory(new MetaBuilder().Build(), typeof(User));
            this.InternalWorkspace = new InternalWorkspace(objectFactory);

            this.ContextFactory = new ContextFactory(this.Database, this.InternalWorkspace, new WorkspaceState());

            this.M = this.ContextFactory.Scope().M;

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
