// <copyright file="PullExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors.Api.Json
{
    using Domain;
    using Protocol.Data;
    using Protocol.Remote.Pull;
    using Protocol.Remote.Push;
    using Protocol.Remote.Sync;
    using Pull;
    using Push;
    using Server;
    using Services;
    using Sync;

    public class Api
    {
        public Api(ISession session, string workspaceName)
        {
            this.Session = session;
            this.WorkspaceName = workspaceName;

            this.TreeService = session.Database.Scope().TreeService;
            this.FetchService = session.Database.Scope().FetchService;
            this.ExtentService = session.Database.Scope().ExtentService;
        }

        public ISession Session { get; }

        public string WorkspaceName { get; }

        public ITreeService TreeService { get; set; }

        public IFetchService FetchService { get; set; }

        public IExtentService ExtentService { get; set; }

        public PullResponse Pull(PullRequest request)
        {
            var acls = new WorkspaceAccessControlLists(this.Session.Scope().User);
            var response = new PullResponseBuilder(acls, this.TreeService);

            if (request.P != null)
            {
                foreach (var p in request.P)
                {
                    var pull = p.Load(this.Session);

                    if (pull.Object != null)
                    {
                        var pullInstantiate = new PullInstantiate(this.Session, pull, acls, this.FetchService);
                        pullInstantiate.Execute(response);
                    }
                    else
                    {
                        var pullExtent = new PullExtent(this.Session, pull, acls, this.ExtentService, this.FetchService);
                        pullExtent.Execute(response);
                    }
                }
            }

            return response.Build();
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            var user = this.Session.Scope().User;
            var acls = new WorkspaceAccessControlLists(user);
            var responseBuilder = new PushResponseBuilder(this.Session, pushRequest, acls);
            return responseBuilder.Build();
        }

        public SyncResponse Sync(SyncRequest syncRequest)
        {
            var acls = new WorkspaceAccessControlLists(this.Session.Scope().User);
            var responseBuilder = new SyncResponseBuilder(this.Session, syncRequest, acls);
            return responseBuilder.Build();
        }
    }
}
