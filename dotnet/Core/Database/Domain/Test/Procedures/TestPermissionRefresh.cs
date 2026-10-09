// <copyright file="TestPermissionRefresh.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public class TestPermissionRefresh : IProcedure
    {
        public void Execute(IProcedureContext context, IProcedureInput input, IProcedureOutput output)
        {
            var action = input.GetValue("action");
            var target = input.GetObject<IObject>("target");

            switch (action)
            {
                case "removeGrantRead":
                    if (target is not C1 c1)
                    {
                        throw new ArgumentException("removeGrantRead requires a C1 object in 'target'.");
                    }

                    var permissionId = c1.Strategy.Class.ReadPermissionIdByRelationTypeId[c1.Meta.C1AllorsString.RelationType.Id];
                    var permission = (Permission)context.Transaction.Instantiate(permissionId);
                    var grants = context.AccessControl[c1].Grants
                        .Select(grant => (Grant)context.Transaction.Instantiate(grant.Id))
                        .ToArray();
                    if (!grants.Any(grant => grant.EffectivePermissions.Contains(permission)))
                    {
                        throw new InvalidOperationException("The target C1 must have a grant for reading C1AllorsString before removing it.");
                    }

                    foreach (var grant in grants)
                    {
                        grant.Role.RemovePermission(permission);
                    }

                    c1.C1AllorsString = "Changed after the read permission was removed";
                    break;

                case "addReadRevocation":
                case "removeReadRevocation":
                    if (target is not Denied denied)
                    {
                        throw new ArgumentException($"{action} requires a Denied object in 'target'.");
                    }

                    var deniedPermissionId = denied.Strategy.Class.ReadPermissionIdByRelationTypeId[denied.Meta.DefaultWorkspaceProperty.RelationType.Id];
                    var deniedPermission = (Permission)context.Transaction.Instantiate(deniedPermissionId);
                    var revocation = denied.Revocations.Single();
                    if (action == "addReadRevocation")
                    {
                        revocation.AddDeniedPermission(deniedPermission);
                        denied.DefaultWorkspaceProperty = "Read denied";
                    }
                    else
                    {
                        revocation.RemoveDeniedPermission(deniedPermission);
                        denied.DefaultWorkspaceProperty = "Read restored";
                    }

                    break;

                default:
                    throw new ArgumentException($"Unknown permission refresh action '{action}'. Use removeGrantRead, addReadRevocation, or removeReadRevocation.");
            }

            var validation = context.Transaction.Derive();
            if (validation.HasErrors)
            {
                context.AddError(validation);
                return;
            }

            context.Transaction.Commit();
        }
    }
}
