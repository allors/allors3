// <copyright file="IDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Threading.Tasks;
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Sync;
    using Protocol.Database.Security;

    public interface IDatabase
    {
        Task<PullResponse> Pull(PullRequest pullRequest);

        Task<PullResponse> Pull(string service, object args);

        Task<SyncResponse> Sync(SyncRequest syncRequest);

        Task<PushResponse> Push(PushRequest pushRequest);

        Task<InvokeResponse> Invoke(InvokeRequest invokeRequest, InvokeOptions options = null);

        Task<InvokeResponse> Invoke(string service, object args);

        Task<SecurityResponse> Security(SecurityRequest securityRequest);
    }
}
