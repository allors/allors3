// <copyright file="PushToDatabaseTracker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;

    internal sealed class PushToDatabaseTracker
    {
        internal ISet<Strategy> Created { get; private set; }

        internal ISet<DatabaseOriginState> Changed { get; private set; }

        private readonly Session session;

        internal PushToDatabaseTracker(Session session) => this.session = session;

        internal void OnCreated(Strategy strategy) => _ = (this.Created ??= new HashSet<Strategy>()).Add(strategy);

        internal void OnChanged(DatabaseOriginState state) => _ = (this.Changed ??= new HashSet<DatabaseOriginState>()).Add(state);

        public PushResult Push(PushResult result)
        {
            result.Execute(this);

            if (result.HasErrors)
            {
                return result;
            }

            if (this.Created?.Count > 0)
            {
                throw new Exception("Not all new objects received ids");
            }

            this.session.OnPush(result);

            this.Created = null;
            this.Changed = null;

            return result;
        }
    }
}
