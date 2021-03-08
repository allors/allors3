// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Meta;

    public sealed class RemoteDiff
    {
        public RemoteDiff(IReadOnlyDictionary<IRelationType, object> roleByRelationType, IReadOnlyDictionary<IRelationType, object> workspaceObjectRoleByRelationType)
        {
        }
    }
}
