// <copyright file="DatabaseOriginState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Ranges;

    internal sealed class DatabaseOriginState : Adapters.DatabaseOriginState
    {
        internal DatabaseOriginState(Strategy strategy, DatabaseRecord record) : base(record) => this.RemoteStrategy = strategy;

        public override Adapters.Strategy Strategy => this.RemoteStrategy;
        private Strategy RemoteStrategy { get; }

        internal bool CanAcceptScoped(DatabaseRecord record) => this.CanAcceptPulledRecord(record);

        internal void AcceptScoped(DatabaseRecord record) => this.AcceptPulledRecord(record);

        public override void Checkpoint(ChangeSet changeSet)
        {
            var owner = this.RemoteStrategy.Session.Workspace.DatabaseConnection.ScopedOwner;
            lock (owner.State)
            {
                owner.RequireActive(this.RemoteStrategy.Session);
                base.Checkpoint(changeSet);
            }
        }

        protected override string UnloadedRelationMessage => "The related object is not loaded in this Session. Include it, add a dependency, or Pull it explicitly before navigating this relation.";

        protected override bool ExactCheckpointRecords => true;

        public override bool CanRead(Meta.IRoleType roleType)
        {
            var owner = this.RemoteStrategy.Session.Workspace.DatabaseConnection.ScopedOwner;
            return owner.IsPermitted(this.RemoteStrategy.Session, this.Id, this.Class, roleType, Allors.Operations.Read, !this.ExistRecord);
        }

        public override bool CanWrite(Meta.IRoleType roleType)
        {
            var owner = this.RemoteStrategy.Session.Workspace.DatabaseConnection.ScopedOwner;
            var permitted = owner.IsPermitted(this.RemoteStrategy.Session, this.Id, this.Class, roleType, Allors.Operations.Write, !this.IsPushed && (!this.ExistRecord));
            return !this.IsPushed && permitted;
        }

        public override bool CanExecute(Meta.IMethodType methodType)
        {
            var owner = this.RemoteStrategy.Session.Workspace.DatabaseConnection.ScopedOwner;
            return owner.IsPermitted(this.RemoteStrategy.Session, this.Id, this.Class, methodType, Allors.Operations.Execute, !this.ExistRecord);
        }

        internal PushRequestNewObject PushNew() => new PushRequestNewObject
        {
            w = this.Id,
            t = this.Class.Tag,
            r = this.PushRoles()
        };

        internal PushRequestObject PushExisting() => new PushRequestObject
        {
            d = this.Id,
            v = this.Version,
            r = this.PushRoles()
        };

        private PushRequestRole[] PushRoles()
        {
            if (this.ChangedRoleByRelationType?.Count > 0)
            {
                var database = this.RemoteStrategy.Session.Workspace.DatabaseConnection;
                var ranges = database.Ranges;
                var roles = new List<PushRequestRole>();

                foreach (var keyValuePair in this.ChangedRoleByRelationType)
                {
                    var relationType = keyValuePair.Key;
                    var roleValue = keyValuePair.Value;

                    var pushRequestRole = new PushRequestRole { t = relationType.Tag };

                    if (relationType.RoleType.ObjectType.IsUnit)
                    {
                        pushRequestRole.u = database.UnitConvert.ToJson(roleValue);
                    }
                    else if (relationType.RoleType.IsOne)
                    {
                        pushRequestRole.c = ((Adapters.Strategy)roleValue)?.Id;
                    }
                    else
                    {
                        var roleIds = ranges.Load(((IRange<Adapters.Strategy>)roleValue)?.Select(v => v.Id));

                        if (!this.ExistRecord)
                        {
                            pushRequestRole.a = roleIds.Save();
                        }
                        else
                        {
                            var databaseRole = ranges.Ensure(this.DatabaseRecord.GetRole(relationType.RoleType));
                            if (databaseRole.IsEmpty)
                            {
                                pushRequestRole.a = roleIds.Save();
                            }
                            else
                            {
                                pushRequestRole.a = ranges.Except(roleIds, databaseRole).Save();
                                pushRequestRole.r = ranges.Except(databaseRole, roleIds).Save();
                            }
                        }
                    }

                    roles.Add(pushRequestRole);
                }

                return roles.ToArray();
            }

            return null;
        }
    }
}
