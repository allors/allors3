// <copyright file="RemoteCallResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api.Invoke;

    public class RemoteCallResult : RemoteResult, ICallResult
    {
        private readonly InvokeResponse invokeResponse;

        public RemoteCallResult(InvokeResponse invokeResponse) : base(invokeResponse) => this.invokeResponse = invokeResponse;
    }
}
