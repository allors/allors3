// <copyright file="IReadTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>The strict codec boundary used by the opt-in scoped coordinator.</summary>
    public interface IReadTransport
    {
        ReadRequest CaptureRequest(ReadRequest request);
        object CaptureResponse(object response, ReadRequest request);
        int RequestByteCount(ReadRequest request);
        Task<object> Read(ReadRequest request, CancellationToken cancellationToken = default);
    }
}
