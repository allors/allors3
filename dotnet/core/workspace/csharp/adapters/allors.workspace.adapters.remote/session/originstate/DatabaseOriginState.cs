// <copyright file="DatabaseOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Allors.Protocol.Json.Api.Push;

    internal sealed class DatabaseOriginState : Adapters.DatabaseOriginState
    {
        internal DatabaseOriginState(Strategy strategy, DatabaseRecord record) : base(record) => this.RemoteStrategy = strategy;

        public override Adapters.Strategy Strategy => this.RemoteStrategy;
        private Strategy RemoteStrategy { get; }

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

                var numbers = database.Numbers;

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
                        pushRequestRole.c = (long?)roleValue;
                    }
                    else
                    {
                        if (!this.ExistDatabaseRecord)
                        {
                            pushRequestRole.a = numbers.ToArray(roleValue);
                        }
                        else
                        {
                            var databaseRole = (long[])this.DatabaseRecord.GetRole(relationType.RoleType);
                            if (databaseRole == null)
                            {
                                pushRequestRole.a = numbers.ToArray(roleValue);
                            }
                            else
                            {
                                pushRequestRole.a = numbers.ToArray(numbers.Except(roleValue, databaseRole));
                                pushRequestRole.r = numbers.ToArray(numbers.Except(databaseRole, roleValue));
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
