// <copyright file="PushTracker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    internal sealed class PushToWorkspaceTracker
    {
        internal ISet<Strategy> Created { get; set; }

        internal ISet<WorkspaceOriginState> Changed { get; set; }

        internal void OnCreated(Strategy strategy) => _ = (this.Created ??= new HashSet<Strategy>()).Add(strategy);

        internal void OnChanged(WorkspaceOriginState state) =>
            _ = (this.Changed ??= new HashSet<WorkspaceOriginState>()).Add(state);
    }
}
