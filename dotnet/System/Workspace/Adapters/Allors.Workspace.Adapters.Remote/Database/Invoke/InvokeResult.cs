// <copyright file="RemoteInvokeResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api.Invoke;

    public class InvokeResult : Result, IInvokeResult
    {
        public InvokeResult(ISession session, InvokeResponse invokeResponse) : base(session, invokeResponse)
        {
        }
    }
}
