// <copyright file="ITransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;

    /// <summary>
    /// Carries the requests of a connection to the server and brings its responses back. The
    /// requests and responses are the JSON protocol; a transport decides how they travel: over
    /// HTTP, or in-process. The connection and its transports are the only users of the
    /// protocol classes.
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// Converts units between their value and their form on the wire, as this transport's
        /// serializer delivers them.
        /// </summary>
        IUnitConvert UnitConvert { get; }

        /// <summary>
        /// The messages the server sends on its own, for a transport that keeps a stream open;
        /// null for a request-response transport such as HTTP.
        /// </summary>
        IObservable<ServerMessage> ServerMessages { get; }

        Task<PullResponse> PullAsync(PullRequest pullRequest);

        /// <summary>
        /// A named pull: a route of the server that takes its own arguments.
        /// </summary>
        Task<PullResponse> PullAsync(string name, object args);

        Task<SyncResponse> SyncAsync(SyncRequest syncRequest);

        Task<PushResponse> PushAsync(PushRequest pushRequest);

        Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest);

        Task<AccessResponse> AccessAsync(AccessRequest accessRequest);

        Task<PermissionResponse> PermissionAsync(PermissionRequest permissionRequest);
    }
}
