// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using Meta;

    internal sealed class WorkspaceOriginState : RecordBasedOriginState
    {
        internal WorkspaceOriginState(Strategy strategy, IRecord record) : base(strategy, record) { }

        protected override void OnChange() => this.Strategy.Session.OnChange(this);

        public override bool HasChanges => this.ChangedRoleByRelationType?.Count > 0;


        internal override void Reset()
        {
            this.Record = this.Workspace.Get(this.Id);
            this.ChangedRoleByRelationType = null;
        }
    }
}
