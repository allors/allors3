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

    [Collection("Remote")]
    public class Test : IDisposable
    {
        public const string Url = "http://localhost:5000";

        public const string InitUrl = "/Test/Init";
        public const string SetupUrl = "/Test/Setup?databaseOrigin=full";
        public const string LoginUrl = "/Test/Login";

        public Workspace Workspace { get; }

        public Allors.Workspace.Adapters.Remote.Remote Remote { get; }

        public DatabaseOrigin DatabaseOrigin { get; }

        public M M { get; }

        public Test()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(Url),
            };

            this.Remote = new Allors.Workspace.Adapters.Remote.Remote(client);
            var objectFactory = new ObjectFactory(new MetaBuilder().Build(), typeof(User));
            this.DatabaseOrigin = new DatabaseOrigin(objectFactory);

            this.Workspace = new Workspace(this.Remote, this.DatabaseOrigin, new WorkspaceStateState());

            this.M = this.Workspace.State().M;

            this.Init();
        }

        public void Dispose()
        {
        }

        public void Login(string user)
        {
            var uri = new Uri("/TestAuthentication/Token", UriKind.Relative);
            var result = this.Remote.Login(uri, user, null).Result;
        }

        private void Init()
        {
            var httpResponseMessage = this.Remote.HttpClient.GetAsync(SetupUrl).Result;
            this.Login("administrator");
        }
    }
}
