// <copyright file="IInvokeReadTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.InvokeRead;

    /// <summary>Optional strict, single-attempt mutation capability; raw responses preserve independent receipts.</summary>
    public interface IInvokeReadTransport
    {
        InvokeReadRequest CaptureRequest(InvokeReadRequest request);
        Task<string> Send(InvokeReadRequest request, CancellationToken cancellationToken = default);
        InvokeReadReceipt CaptureReceipt(string response, InvokeReadRequest request);
        object CapturePart(string response, InvokeReadRequest request);
    }
}
