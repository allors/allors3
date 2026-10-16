// <copyright file="TestPermissionController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System.Linq;
    using Database;
    using Database.Domain;
    using Database.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Services;

    [Authorize]
    [ApiController]
    [Route("allors/test/permissions")]
    public class TestPermissionController : ControllerBase
    {
        private readonly ITransaction transaction;

        public TestPermissionController(ITransactionService transactionService) => this.transaction = transactionService.Transaction;

        [HttpPost("remove-c1-string-read-grant")]
        public IActionResult RemoveC1StringReadGrant([FromQuery] long targetId)
        {
            var error = this.ValidateTarget<C1>(targetId, out var target);
            if (error != null)
            {
                return error;
            }

            var permissionId = target.Strategy.Class.ReadPermissionIdByRelationTypeId[target.Meta.C1AllorsString.RelationType.Id];
            var permission = (Permission)this.transaction.Instantiate(permissionId);
            var accessControl = this.transaction.Services.Get<IWorkspaceAclsService>().Create("Default");
            var grants = accessControl[target].Grants
                .Select(grant => (Grant)this.transaction.Instantiate(grant.Id))
                .ToArray();
            if (!grants.Any(grant => grant.EffectivePermissions.Contains(permission)))
            {
                return this.BadRequest("The target C1 must have an effective grant for reading C1AllorsString for the current user in Default before removing it.");
            }

            foreach (var grant in grants)
            {
                grant.Role.RemovePermission(permission);
            }

            target.C1AllorsString = "Changed after the read permission was removed";
            return this.Finish();
        }

        [HttpPost("add-denied-property-read-revocation")]
        public IActionResult AddDeniedPropertyReadRevocation([FromQuery] long targetId)
        {
            var error = this.ValidateRevocation(targetId, out var target, out var revocation, out var permission);
            if (error != null)
            {
                return error;
            }

            revocation.AddDeniedPermission(permission);
            target.DefaultWorkspaceProperty = "Read denied";
            return this.Finish();
        }

        [HttpPost("remove-denied-property-read-revocation")]
        public IActionResult RemoveDeniedPropertyReadRevocation([FromQuery] long targetId)
        {
            var error = this.ValidateRevocation(targetId, out var target, out var revocation, out var permission);
            if (error != null)
            {
                return error;
            }

            revocation.RemoveDeniedPermission(permission);
            target.DefaultWorkspaceProperty = "Read restored";
            return this.Finish();
        }

        private IActionResult ValidateTarget<T>(long targetId, out T target) where T : class, IObject
        {
            target = null;
            if (this.transaction.Services.Get<IUserService>().User == null)
            {
                return this.Unauthorized();
            }

            if (targetId <= 0)
            {
                return this.BadRequest("targetId must be the positive ID of a committed object.");
            }

            target = this.transaction.Instantiate(targetId) as T;
            return target == null
                ? this.BadRequest($"targetId must identify an existing {typeof(T).Name} object.")
                : null;
        }

        private IActionResult ValidateRevocation(long targetId, out Denied target, out Revocation revocation, out Permission permission)
        {
            revocation = null;
            permission = null;
            var error = this.ValidateTarget(targetId, out target);
            if (error != null)
            {
                return error;
            }

            var revocations = target.Revocations.ToArray();
            if (revocations.Length != 1)
            {
                return this.BadRequest("The target Denied must have exactly one Revocation before changing its DefaultWorkspaceProperty read permission.");
            }

            revocation = revocations[0];
            var permissionId = target.Strategy.Class.ReadPermissionIdByRelationTypeId[target.Meta.DefaultWorkspaceProperty.RelationType.Id];
            permission = (Permission)this.transaction.Instantiate(permissionId);
            return null;
        }

        private IActionResult Finish()
        {
            var validation = this.transaction.Derive(throwExceptionOnError: false);
            if (validation.HasErrors)
            {
                var message = "Permission setup failed validation: " + string.Join("; ", validation.Errors.Select(error => error.Message));
                this.transaction.Rollback();
                return this.BadRequest(message);
            }

            this.transaction.Commit();
            return this.NoContent();
        }
    }
}
