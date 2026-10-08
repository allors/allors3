// <copyright file="DatabaseOriginState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System.Collections.Generic;
    using System.Linq;
    using Connection;
    using Meta;
    using Ranges;

    public sealed class DatabaseOriginState : RecordBasedOriginState
    {
        internal DatabaseOriginState(Strategy strategy, IRecord record)
        {
            this.Strategy = strategy;
            this.DatabaseRecord = record;
            this.PreviousRecord = this.DatabaseRecord;
            this.IsPushed = false;
        }

        public override Strategy Strategy { get; }

        public long Version => this.DatabaseRecord?.Version ?? Allors.Version.WorkspaceInitial;

        private bool IsVersionInitial => this.Version == Allors.Version.WorkspaceInitial.Value;

        protected override IEnumerable<IRoleType> RoleTypes => this.Class.DatabaseOriginRoleTypes;

        protected override IRecord Record => this.DatabaseRecord;

        private bool ExistRecord => this.Record != null;

        private IRecord DatabaseRecord { get; set; }

        private bool IsPushed { get; set; }

        private IDatabaseConnection Connection => this.Session.Workspace.Connection;

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

            var permission = this.Connection.GetPermission(this.Class, roleType, Operations.Read);
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

            var permission = this.Connection.GetPermission(this.Class, roleType, Operations.Write);
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

            var permission = this.Connection.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public void OnPushed() => this.IsPushed = true;

        public void OnPulled(IPullResultInternals pull)
        {
            var newRecord = this.Connection.GetRecord(this.Id);

            if (!this.IsPushed)
            {
                if (!this.CanMerge(newRecord))
                {
                    pull.AddMergeError(this.Strategy.Object);
                    return;
                }
            }
            else
            {
                this.Reset();
                this.IsPushed = false;
            }

            this.DatabaseRecord = newRecord;
        }

        internal PushNewObject PushNew() => new PushNewObject(this.Id, this.Class, this.RoleChanges());

        internal PushChangedObject PushExisting() => new PushChangedObject(this.Id, this.Version, this.RoleChanges());

        protected override void OnChange()
        {
            this.Session.ChangeSetTracker.OnDatabaseChanged(this);
            this.Session.PushToDatabaseTracker.OnChanged(this);
        }

        /// <summary>
        /// The changed roles in the shape the connection takes: a unit, the id of a composite
        /// role, or the ids of a composites role.
        /// </summary>
        private RoleChange[] RoleChanges()
        {
            if (!(this.ChangedRoleByRelationType?.Count > 0))
            {
                return null;
            }

            var roleChanges = new List<RoleChange>();

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                var roleType = kvp.Key.RoleType;
                var roleValue = kvp.Value;

                object value;
                if (roleType.ObjectType.IsUnit)
                {
                    value = roleValue;
                }
                else if (roleType.IsOne)
                {
                    value = ((Strategy)roleValue)?.Id;
                }
                else
                {
                    value = ((IRange<Strategy>)roleValue)?.Select(v => v.Id);
                }

                roleChanges.Add(new RoleChange(roleType, value));
            }

            return roleChanges.ToArray();
        }
    }
}
