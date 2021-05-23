// <copyright file="DatabaseOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
            this.PreviousRecord = this.DatabaseRecord;
        }

        public long Version => this.DatabaseRecord.Version;

        protected bool ExistDatabaseRecord => this.Record != null;

        protected override IEnumerable<IRoleType> RoleTypes => this.Class.DatabaseOriginRoleTypes;

        protected override IRecord Record => this.DatabaseRecord;

        protected DatabaseRecord DatabaseRecord { get; set; }

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission =
                this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission =
                this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission =
                this.Session.Workspace.DatabaseConnection.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRecord.IsPermitted(permission);
        }
        
        public void PushResponse(DatabaseRecord newDatabaseRecord) => this.DatabaseRecord = newDatabaseRecord;

        protected override void OnChange()
        {
            this.Session.ChangeSetTracker.OnChanged(this);
            this.Session.PushToDatabaseTracker.OnChanged(this);
        }
    }
}
