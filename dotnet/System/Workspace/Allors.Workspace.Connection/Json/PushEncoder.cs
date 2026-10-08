// <copyright file="PushEncoder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Push;
    using Ranges;

    /// <summary>
    /// Encodes the objects of a push for the wire. A composites role is sent as the ids to add
    /// and the ids to remove against the record the cache holds; without a record, as for a new
    /// object, every id is an addition. The server compares the version sent with its own, so a
    /// record newer than the version sent fails the push before the roles are applied.
    /// </summary>
    internal sealed class PushEncoder
    {
        private readonly ICache cache;
        private readonly IUnitConvert unitConvert;
        private readonly IRanges<long> ranges;

        internal PushEncoder(ICache cache, IUnitConvert unitConvert, IRanges<long> ranges)
        {
            this.cache = cache;
            this.unitConvert = unitConvert;
            this.ranges = ranges;
        }

        internal PushRequestNewObject ToJson(PushNewObject newObject) => new PushRequestNewObject
        {
            w = newObject.WorkspaceId,
            t = newObject.Class.Tag,
            r = this.Roles(newObject.Roles, null),
        };

        internal PushRequestObject ToJson(PushChangedObject changedObject) => new PushRequestObject
        {
            d = changedObject.Id,
            v = changedObject.Version,
            r = this.Roles(changedObject.Roles, this.cache.GetRecord(changedObject.Id)),
        };

        private PushRequestRole[] Roles(RoleChange[] roleChanges, IRecord record)
        {
            if (roleChanges == null || roleChanges.Length == 0)
            {
                return null;
            }

            var roles = new List<PushRequestRole>();

            foreach (var roleChange in roleChanges)
            {
                var roleType = roleChange.RoleType;
                var value = roleChange.Value;

                var pushRequestRole = new PushRequestRole { t = roleType.RelationType.Tag };

                if (roleType.ObjectType.IsUnit)
                {
                    pushRequestRole.u = this.unitConvert.ToJson(value);
                }
                else if (roleType.IsOne)
                {
                    pushRequestRole.c = (long?)value;
                }
                else
                {
                    var roleIds = this.ranges.Import((IEnumerable<long>)value);

                    if (record == null)
                    {
                        pushRequestRole.a = roleIds.Save();
                    }
                    else
                    {
                        var databaseRole = this.ranges.Ensure(record.GetRole(roleType));
                        if (databaseRole.IsEmpty)
                        {
                            pushRequestRole.a = roleIds.Save();
                        }
                        else
                        {
                            pushRequestRole.a = this.ranges.Except(roleIds, databaseRole).Save();
                            pushRequestRole.r = this.ranges.Except(databaseRole, roleIds).Save();
                        }
                    }
                }

                roles.Add(pushRequestRole);
            }

            return roles.ToArray();
        }
    }
}
