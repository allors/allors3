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
        private readonly Session session;

        private ISet<IStrategy> created;
        private ISet<IStrategy> instantiated;
        private ISet<DatabaseOriginState> databaseOriginStates;
        private ISet<WorkspaceOriginState> workspaceOriginStates;

        internal ChangeSetTracker(Session session) => this.session = session;

        internal void OnCreated(Strategy strategy) => _ = (this.created ??= new HashSet<IStrategy>()).Add(strategy);

        internal void OnInstantiated(Strategy strategy) => _ = (this.instantiated ??= new HashSet<IStrategy>()).Add(strategy);

        internal void OnChanged(DatabaseOriginState state) => _ = (this.databaseOriginStates ??= new HashSet<DatabaseOriginState>()).Add(state);

        internal void OnChanged(WorkspaceOriginState state) => _ = (this.workspaceOriginStates ??= new HashSet<WorkspaceOriginState>()).Add(state);

        internal ChangeSet Checkpoint()
        {
            var changeSet = new ChangeSet(this.session, this.created, this.instantiated);

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

            this.created = null;
            this.instantiated = null;
            this.databaseOriginStates = null;
            this.workspaceOriginStates = null;

            return changeSet;
        }
    }
}
