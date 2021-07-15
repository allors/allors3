// <copyright file="RemoteDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote.ResthSharp
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Auth;
    using Allors.Protocol.Json.RestSharp;
    using Ranges;
    using RestSharp;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1090:Add call to 'ConfigureAwait' (or vice versa).", Justification = "<Pending>")]
    public class DatabaseConnection : Remote.DatabaseConnection
    {
        private readonly Func<IRestClient> restClientFactory;

        public DatabaseConnection(Configuration configuration, Func<IWorkspaceServices> servicesBuilder, Func<IRestClient> restClientFactory, IdGenerator idGenerator, IRanges ranges) : base(configuration, servicesBuilder, idGenerator, ranges)
        {
            this.restClientFactory = restClientFactory;
            this.UnitConvert = new UnitConvert();
        }

        public override IUnitConvert UnitConvert { get; }

        public IRestClient RestClient { get; private set; }

        public int[] SecondsBeforeRetry { get; set; } = { 1, 2, 4, 8, 16 };

        public override async Task<SyncResponse> Sync(SyncRequest syncRequest)
        {
            var uri = new Uri("sync", UriKind.Relative);
            return await this.Post<SyncResponse>(uri, syncRequest);
        }

        public override async Task<InvokeResponse> Invoke(InvokeRequest invokeRequest)
        {
            var uri = new Uri("invoke", UriKind.Relative);
            return await this.Post<InvokeResponse>(uri, invokeRequest);
        }

        public override async Task<SecurityResponse> Security(SecurityRequest securityRequest)
        {
            var uri = new Uri("security", UriKind.Relative);
            return await this.Post<SecurityResponse>(uri, securityRequest);
        }

        public override async Task<PushResponse> Push(PushRequest pushRequest)
        {
            var uri = new Uri("push", UriKind.Relative);
            // TODO: Retry for network errors, but not for server errors
            return await this.PostOnce<PushResponse>(uri, pushRequest);
        }

        public override async Task<PullResponse> Pull(PullRequest pullRequest)
        {
            var uri = new Uri("pull", UriKind.Relative);
            return await this.Post<PullResponse>(uri, pullRequest);
        }

        public async Task<bool> Login(Uri url, string username, string password)
        {
            this.RestClient = this.restClientFactory();

            var data = new AuthenticationTokenRequest { l = username, p = password };
            var result = await this.Post<AuthenticationTokenResponse>(url, data);

            if (!result.a)
            {
                this.RestClient = null;
                return false;
            }

            this.RestClient.AddDefaultHeader("Authorization", $"Bearer {result.t}");
            this.UserId = result.u;

            return true;
        }

        public void Logoff()
        {
            this.RestClient = null;
            this.UserId = null;
        }

        private async Task<T> Post<T>(Uri uri, object data)
        {
            if (this.SecondsBeforeRetry == null || this.SecondsBeforeRetry.Length == 0)
            {
                return await this.PostOnce<T>(uri, data);
            }

            Exception exception = null;

            foreach (var secondBeforeRetry in this.SecondsBeforeRetry)
            {
                try
                {
                    return await this.PostOnce<T>(uri, data);
                }
                catch (Exception e)
                {
                    exception = e;
                    await Task.Delay(secondBeforeRetry);
                }
            }

            throw exception ?? new Exception("Post not executed");
        }

        private async Task<T> PostOnce<T>(Uri uri, object data)
        {
            var request = new RestRequest(uri, Method.POST, DataFormat.Json);
            if (data != null)
            {
                request.AddJsonBody(data);
            }

            return await this.RestClient.PostAsync<T>(request);
        }
    }
}
