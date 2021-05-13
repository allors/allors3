// <copyright file="ChangeSetTracker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;

    internal sealed class ChangeSetTracker
    {
        private ISet<DatabaseOriginState> databaseOriginStates;
        private ISet<WorkspaceOriginState> workspaceOriginStates;

        internal void OnChanged(DatabaseOriginState state) => _ = (this.databaseOriginStates ??= new HashSet<DatabaseOriginState>()).Add(state);

        internal void OnChanged(WorkspaceOriginState state) => _ = (this.workspaceOriginStates ??= new HashSet<WorkspaceOriginState>()).Add(state);

        internal void Checkpoint(ChangeSet changeSet)
        {
            if (this.databaseOriginStates != null)
            {
                foreach (var changed in this.databaseOriginStates)
                {
                    changed.Checkpoint(changeSet);
                }
            }

            if (this.workspaceOriginStates != null)
            {
                foreach (var changed in this.workspaceOriginStates)
                {
                    changed.Checkpoint(changeSet);
                }
            }

            this.databaseOriginStates = null;
            this.workspaceOriginStates = null;
        }
    }
}
