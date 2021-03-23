// <copyright file="RemoteSaveResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api;

    public class RemoteSaveResult : RemoteResult, ISaveResult
    {
        public RemoteSaveResult(ISession session, Response response) : base(session, response)
        {
        }
    }
}
