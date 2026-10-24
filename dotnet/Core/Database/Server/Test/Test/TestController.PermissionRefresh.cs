// <copyright file="TestController.PermissionRefresh.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using System.Linq;
    using Database.Domain;
    using MetaPopulation = Allors.Database.Meta.MetaPopulation;
    using Database.Services;
    using Microsoft.AspNetCore.Mvc;

    public partial class TestController
    {
        [HttpPost]
        public IActionResult PermissionRefresh([FromBody] PermissionRefreshRequest request)
        {
            try
            {
                using var transaction = this.Database.CreateTransaction();
                var m = this.Database.Services.Get<MetaPopulation>();
                transaction.Services.Get<IUserService>().User = new Users(transaction).FindBy(m.User.UniqueId, Users.AdministratorId);
                var accessControl = transaction.Services.Get<IWorkspaceAclsService>().Create("Default");
                var action = request.Action;
                var target = transaction.Instantiate(request.Target);

                switch (action)
                {
                    case "removeGrantRead":
                        if (target is not C1 c1)
                        {
                            throw new ArgumentException("removeGrantRead requires a C1 object in 'target'.");
                        }

                        var permissionId = c1.Strategy.Class.ReadPermissionIdByRelationTypeId[c1.Meta.C1AllorsString.RelationType.Id];
                        var permission = (Permission)transaction.Instantiate(permissionId);
                        var grants = accessControl[c1].Grants
                            .Select(grant => (Grant)transaction.Instantiate(grant.Id))
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

                    case "removeGrantWrite":
                        if (target is not C1 writableC1)
                        {
                            throw new ArgumentException("removeGrantWrite requires a C1 object in 'target'.");
                        }

                        var writePermissionId = writableC1.Strategy.Class.WritePermissionIdByRelationTypeId[writableC1.Meta.C1AllorsString.RelationType.Id];
                        var writePermission = (Permission)transaction.Instantiate(writePermissionId);
                        var writeGrants = accessControl[writableC1].Grants
                            .Select(grant => (Grant)transaction.Instantiate(grant.Id))
                            .ToArray();
                        if (!writeGrants.Any(grant => grant.EffectivePermissions.Contains(writePermission)))
                        {
                            throw new InvalidOperationException("The target C1 must have a grant for writing C1AllorsString before removing it.");
                        }

                        foreach (var grant in writeGrants)
                        {
                            grant.Role.RemovePermission(writePermission);
                        }

                        // Keep the object's version unchanged so a later Push tests access denial.
                        break;

                    case "addReadRevocation":
                    case "removeReadRevocation":
                        if (target is not Denied denied)
                        {
                            throw new ArgumentException($"{action} requires a Denied object in 'target'.");
                        }

                        var deniedPermissionId = denied.Strategy.Class.ReadPermissionIdByRelationTypeId[denied.Meta.DefaultWorkspaceProperty.RelationType.Id];
                        var deniedPermission = (Permission)transaction.Instantiate(deniedPermissionId);
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
                        throw new ArgumentException($"Unknown permission refresh action '{action}'. Use removeGrantRead, removeGrantWrite, addReadRevocation, or removeReadRevocation.");
                }

                var validation = transaction.Derive();
                if (validation.HasErrors)
                {
                    return this.BadRequest("The permission refresh fixture could not derive its changes.");
                }

                transaction.Commit();
                return this.Ok(new { });
            }
            catch (Exception e)
            {
                this.LogActionFailed(e, nameof(this.PermissionRefresh));
                return this.BadRequest(e.Message);
            }
        }

        public class PermissionRefreshRequest
        {
            public long Target { get; set; }

            public string Action { get; set; }
        }
    }
}
