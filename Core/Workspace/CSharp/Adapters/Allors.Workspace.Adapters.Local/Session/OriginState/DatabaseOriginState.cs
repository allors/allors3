// <copyright file="DatabaseOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using Meta;

    internal sealed class DatabaseOriginState : RecordBasedOriginState
    {
        internal DatabaseOriginState(Strategy strategy, DatabaseRecord record) : base(strategy)
        {
            this.DatabaseRecord = record;
            this.PreviousRecord = this.DatabaseRecord;
        }

        public long Version => this.DatabaseRecord.Version;

        private bool ExistDatabaseRecord => this.Record != null;

        protected override IEnumerable<IRoleType> RoleTypes => this.Class.DatabaseOriginRoleTypes;

        protected override IRecord Record => this.DatabaseRecord;

        private DatabaseRecord DatabaseRecord { get; set; }

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        internal void Reset()
        {
            this.DatabaseRecord = this.Session.DatabaseAdapter.GetRecord(this.Id);
            this.ChangedRoleByRelationType = null;
        }

        internal void PushResponse(DatabaseRecord newDatabaseRecord) => this.DatabaseRecord = newDatabaseRecord;

        protected override void OnChange()
        {
            this.Session.ChangeSetTracker.OnChanged(this);
            this.Session.PushToDatabaseTracker.OnChanged(this);
        }
    }
}
