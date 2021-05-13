// <copyright file="PushToWorkspaceTracker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class PushToWorkspaceTracker
    {
        private ISet<Strategy> created;

        private ISet<WorkspaceOriginState> changed;

        internal void OnCreated(Strategy strategy) => _ = (this.created ??= new HashSet<Strategy>()).Add(strategy);

        internal void OnChanged(WorkspaceOriginState state) => _ = (this.changed ??= new HashSet<WorkspaceOriginState>()).Add(state);

        public PushResult Push(PushResult result)
        {
            if (this.created != null)
            {
                foreach (var strategy in this.created)
                {
                    strategy.WorkspaceOriginState.Push();
                }
            }

            foreach (var state in this.changed)
            {
                if (this.created?.Contains(state.Strategy) == true)
                {
                    continue;
                }

                state.Push();
            }

            this.created = null;
            this.changed = null;

            return result;
        }
    }
}
