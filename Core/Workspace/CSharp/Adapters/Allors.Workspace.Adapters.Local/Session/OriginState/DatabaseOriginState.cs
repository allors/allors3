// <copyright file="DatabaseOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    internal sealed class DatabaseOriginState : Adapters.DatabaseOriginState
    {
        internal DatabaseOriginState(Strategy strategy, DatabaseRecord record) : base(record) => this.Strategy = strategy;

        public override Adapters.Strategy Strategy { get; }

        public void OnPulled() =>
            // TODO: check for overwrites
            this.DatabaseRecord = this.Session.Workspace.Database.GetRecord(this.Id);

        public void Reset()
        {
            this.DatabaseRecord = this.Session.Workspace.Database.GetRecord(this.Id);
            this.ChangedRoleByRelationType = null;
        }

    }
}