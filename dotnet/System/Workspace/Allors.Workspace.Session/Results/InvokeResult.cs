// <copyright file="InvokeResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using ConnectionInvokeResult = Allors.Workspace.Connection.InvokeResult;

    public sealed class InvokeResult : Result, IInvokeResult
    {
        internal InvokeResult(ISession session, ConnectionInvokeResult invoked) : base(session, invoked)
        {
        }
    }
}
