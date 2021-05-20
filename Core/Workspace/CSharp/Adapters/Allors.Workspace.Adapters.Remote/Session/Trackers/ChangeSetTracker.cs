// <copyright file="ChangeSetTracker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    internal sealed class ChangeSetTracker
    {
        internal ISet<IStrategy> Created { get; set; }

        internal ISet<IStrategy> Instantiated { get; set; }

        internal ISet<DatabaseOriginState> DatabaseOriginStates { get; set; }

        internal ISet<WorkspaceOriginState> WorkspaceOriginStates { get; set; }

        internal void OnCreated(Strategy strategy) => _ = (this.Created ??= new HashSet<IStrategy>()).Add(strategy);

        internal void OnInstantiated(Strategy strategy) =>
            _ = (this.Instantiated ??= new HashSet<IStrategy>()).Add(strategy);

        internal void OnChanged(DatabaseOriginState state) =>
            _ = (this.DatabaseOriginStates ??= new HashSet<DatabaseOriginState>()).Add(state);

        internal void OnChanged(WorkspaceOriginState state) =>
            _ = (this.WorkspaceOriginStates ??= new HashSet<WorkspaceOriginState>()).Add(state);
    }
}
