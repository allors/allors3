// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Net.Http;

    using Allors.Workspace;
    using Allors.Workspace.Adapters.Remote;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;

    using Xunit;

    [Collection("Database")]
    public class Test : IDisposable
    {
        public const string Url = "http://localhost:5000";

        public const string InitUrl = "/Test/Init";
        public const string SetupUrl = "/Test/Setup?population=full";
        public const string LoginUrl = "/Test/Login";

        public Workspace Workspace { get; }

        public Database Database => this.Workspace.Database;

        public M M => this.Workspace.State().M;

        public Test()
        {
            this.Workspace = new Workspace(
                new MetaBuilder().Build(),
                typeof(User),
                new WorkspaceState(),
                new HttpClient()
                {
                    BaseAddress = new Uri(Url),
                });

            this.Init();
        }

        public void Dispose()
        {
        }

        public void Login(string user)
        {
            var uri = new Uri("/TestAuthentication/Token", UriKind.Relative);
            var response = this.Database.Login(uri, user, null).Result;
            Assert.True(response);
        }

        private void Init()
        {
            var response = this.Database.HttpClient.GetAsync(SetupUrl).Result;
            Assert.True(response.IsSuccessStatusCode);
            this.Login("administrator");
        }
    }
}
