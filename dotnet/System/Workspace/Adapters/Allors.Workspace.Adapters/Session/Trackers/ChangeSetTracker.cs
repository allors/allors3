// <copyright file="ChangeSetTracker.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;

    public sealed class ChangeSetTracker
    {
        public ChangeSetTracker(Session session) => this.Session = session;

        public Session Session { get; set; }

        public ISet<IStrategy> Created { get; set; }

        public ISet<IStrategy> Instantiated { get; set; }

        public ISet<DatabaseOriginState> DatabaseOriginStates { get; set; }

        public ISet<SessionOriginState> SessionOriginStates { get; set; }

        public void OnCreated(Strategy strategy)
        {
            (this.Created ??= new HashSet<IStrategy>()).Add(strategy);
            this.Session.OnChanged(EventArgs.Empty);
        }

        public void OnInstantiated(Strategy strategy)
        {
            (this.Instantiated ??= new HashSet<IStrategy>()).Add(strategy);
            this.Session.OnChanged(EventArgs.Empty);
        }

        public void OnDatabaseChanged(DatabaseOriginState state)
        {
            (this.DatabaseOriginStates ??= new HashSet<DatabaseOriginState>()).Add(state);
            this.Session.OnChanged(EventArgs.Empty);
        }

        public void OnSessionChanged(SessionOriginState state)
        {
            (this.SessionOriginStates ??= new HashSet<SessionOriginState>()).Add(state);
            this.Session.OnChanged(EventArgs.Empty);
        }
    }
}
