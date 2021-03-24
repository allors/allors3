// <copyright file="RemoteInvokeResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api.Invoke;

    public class RemoteInvokeResult : RemoteResult, IInvokeResult
    {
        private readonly InvokeResponse invokeResponse;

        public RemoteInvokeResult(ISession session, InvokeResponse invokeResponse) : base(session, invokeResponse) => this.invokeResponse = invokeResponse;
    }
}
