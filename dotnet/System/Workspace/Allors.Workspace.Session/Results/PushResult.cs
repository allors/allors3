// <copyright file="PushResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using ConnectionPushResult = Allors.Workspace.Connection.PushResult;

    public sealed class PushResult : Result, IPushResult
    {
        internal PushResult(ISession session, ConnectionPushResult pushed) : base(session, pushed)
        {
        }
    }
}
