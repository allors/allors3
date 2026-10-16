// <copyright file="IPushReadTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.PushRead;

    /// <summary>Optional strict, single-attempt mutation capability; raw responses preserve independent receipts.</summary>
    public interface IPushReadTransport
    {
        PushReadRequest CaptureRequest(PushReadRequest request);
        Task<string> Send(PushReadRequest request, CancellationToken cancellationToken = default);
        PushReadReceipt CaptureReceipt(string response, PushReadRequest request);
        object CapturePart(string response, PushReadRequest request, bool changed);
    }
}
