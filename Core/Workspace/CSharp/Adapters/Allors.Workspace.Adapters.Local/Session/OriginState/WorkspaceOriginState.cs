// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    internal sealed class WorkspaceOriginState : RecordBasedOriginState
    {
        internal WorkspaceOriginState(Strategy strategy, WorkspaceRecord record) : base(strategy)
        {
            this.WorkspaceRecord = record;
            this.PreviousRecord = this.WorkspaceRecord;
        }

        protected override IRecord Record => this.WorkspaceRecord;

        private WorkspaceRecord WorkspaceRecord { get; set; }

        protected override void OnChange() => this.Strategy.Session.OnChange(this);

        private bool HasChanges => this.ChangedRoleByRelationType?.Count > 0;

        internal void Push()
        {
            if (this.HasChanges)
            {
                this.Workspace.Push(this.Id, this.Class, this.Record?.Version ?? 0, this.ChangedRoleByRelationType);
            }

            this.Reset();
        }

        internal void Reset()
        {
            this.WorkspaceRecord = this.Workspace.GetRecord(this.Id);
            this.ChangedRoleByRelationType = null;
        }
    }
}
