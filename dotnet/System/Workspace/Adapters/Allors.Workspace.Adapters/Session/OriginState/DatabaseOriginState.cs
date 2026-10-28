// <copyright file="DatabaseOriginState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using Meta;

    public abstract class DatabaseOriginState : RecordBasedOriginState
    {
        protected DatabaseOriginState(DatabaseRecord record)
        {
            this.DatabaseRecord = record;
            this.LatestReceivedRecord = record;
            this.PreviousRecord = this.DatabaseRecord;
            this.IsPushed = false;
        }

        public long Version => this.DatabaseRecord?.Version ?? Allors.Version.WorkspaceInitial;

        private bool IsVersionInitial => this.Version == Allors.Version.WorkspaceInitial.Value;

        protected override IEnumerable<IRoleType> RoleTypes => this.Class.DatabaseOriginRoleTypes;

        protected bool ExistRecord => this.Record != null;

        protected override IRecord Record => this.DatabaseRecord;

        protected DatabaseRecord DatabaseRecord { get; private set; }

        private DatabaseRecord LatestReceivedRecord { get; set; }

        private bool IsPushed { get; set; }

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistRecord)
            {
                return true;
            }

            if (this.IsVersionInitial)
            {
                // TODO: Security
                return true;
            }

            var permission = this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (this.IsVersionInitial)
            {
                return !this.IsPushed;
            }

            if (this.IsPushed)
            {
                return false;
            }

            if (!this.ExistRecord)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistRecord)
            {
                return true;
            }

            if (this.IsVersionInitial)
            {
                // TODO: Security
                return true;
            }

            var permission = this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public void OnPushed() => this.IsPushed = true;

        public void OnPulled(DatabaseRecord record)
        {
            this.LatestReceivedRecord = record;
            if (this.IsPushed)
            {
                base.Reset();
                this.IsPushed = false;
            }
            else if (this.ChangedRoleByRelationType?.Count > 0)
            {
                return;
            }

            this.DatabaseRecord = record;
            this.Session.ChangeSetTracker.OnDatabaseChanged(this);
        }

        public override void Reset()
        {
            base.Reset();
            this.Session.PushToDatabaseTracker.Changed?.Remove(this);
            this.DatabaseRecord = this.LatestReceivedRecord;
            this.Session.ChangeSetTracker.OnDatabaseChanged(this);
        }

        protected override void OnChange()
        {
            this.Session.ChangeSetTracker.OnDatabaseChanged(this);
            this.Session.PushToDatabaseTracker.OnChanged(this);
        }
    }
}
