// <copyright file="DatabaseOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
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

            var permission =
                this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission =
                this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseRecord)
            {
                return true;
            }

            var permission =
                this.Session.Workspace.Database.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRecord.IsPermitted(permission);
        }

        internal void OnPulled()
        {
            // TODO: check for overwrites
            this.DatabaseRecord = this.Session.Database.GetRecord(this.Id);
        }

        internal void Reset()
        {
            this.DatabaseRecord = this.Session.Database.GetRecord(this.Id);
            this.ChangedRoleByRelationType = null;
        }

        internal PushRequestNewObject PushNew() => new PushRequestNewObject
        {
            WorkspaceId = this.Id,
            ObjectType = this.Class.Tag,
            Roles = this.PushRoles(),
        };

        internal PushRequestObject PushExisting() => new PushRequestObject
        {
            DatabaseId = this.Id,
            Version = this.Version,
            Roles = this.PushRoles(),
        };

        private PushRequestRole[] PushRoles()
        {
            if (this.ChangedRoleByRelationType?.Count > 0)
            {
                var roles = new List<PushRequestRole>();

                foreach (var keyValuePair in this.ChangedRoleByRelationType)
                {
                    var relationType = keyValuePair.Key;
                    var roleValue = keyValuePair.Value;

                    var pushRequestRole = new PushRequestRole { RelationType = relationType.Tag };

                    if (relationType.RoleType.ObjectType.IsUnit)
                    {
                        pushRequestRole.SetUnitRole = UnitConvert.ToJson(roleValue);
                    }
                    else
                    {
                        if (relationType.RoleType.IsOne)
                        {
                            pushRequestRole.SetCompositeRole = ((Strategy)roleValue)?.Id;
                        }
                        else
                        {
                            var roleIds = ((Strategy[])roleValue).Select(v => v.Id).ToArray();
                            if (!this.ExistDatabaseRecord)
                            {
                                pushRequestRole.AddCompositesRole = roleIds;
                            }
                            else
                            {
                                var databaseRole = (long[])this.DatabaseRecord.GetRole(relationType.RoleType);
                                if (databaseRole == null)
                                {
                                    pushRequestRole.AddCompositesRole = roleIds;
                                }
                                else
                                {
                                    pushRequestRole.AddCompositesRole = roleIds.Except(databaseRole).ToArray();
                                    pushRequestRole.RemoveCompositesRole = databaseRole.Except(roleIds).ToArray();
                                }
                            }
                        }
                    }

                    roles.Add(pushRequestRole);
                }

                return roles.ToArray();
            }

            return null;
        }

        internal void PushResponse(DatabaseRecord newDatabaseRecord) => this.DatabaseRecord = newDatabaseRecord;

        protected override void OnChange()
        {
            this.Session.ChangeSetTracker.OnChanged(this);
            this.Session.PushToDatabaseTracker.OnChanged(this);
        }
    }
}
