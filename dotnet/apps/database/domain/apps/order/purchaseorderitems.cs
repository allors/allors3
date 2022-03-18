// <copyright file="PurchaseOrderItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.PurchaseOrderItemState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var itemStates = new PurchaseOrderItemStates(this.Transaction);
            var created = itemStates.Created;
            var onHold = itemStates.OnHold;
            var cancelled = itemStates.Cancelled;
            var rejected = itemStates.Rejected;
            var awaitingApproval = itemStates.AwaitingApproval;
            var inProcess = itemStates.InProcess;
            var completed = itemStates.Completed;
            var finished = itemStates.Finished;
            var sent = itemStates.Sent;

            var cancel = this.Meta.Cancel;
            var reject = this.Meta.Reject;
            var quickReceive = this.Meta.QuickReceive;
            var @return = this.Meta.Return;
            var reopen = this.Meta.Reopen;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, created, quickReceive, reopen, @return);
            config.Deny(this.ObjectType, onHold, quickReceive, delete, reopen, @return);
            config.Deny(this.ObjectType, awaitingApproval, cancel, reject, quickReceive, delete, reopen, @return);
            config.Deny(this.ObjectType, inProcess, cancel, reject, delete, quickReceive, delete, reopen);
            config.Deny(this.ObjectType, completed, cancel, reject, delete);
            config.Deny(this.ObjectType, cancelled, cancel, reject, quickReceive, @return);
            config.Deny(this.ObjectType, rejected, cancel, reject, quickReceive, @return);

            config.Deny(this.ObjectType, inProcess, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Write);

            var except = new HashSet<IOperandType>
            {
                this.Meta.Return,
            };

            config.DenyExcept(this.ObjectType, completed, except, Operations.Execute, Operations.Write);
            config.DenyExcept(this.ObjectType, finished, except, Operations.Execute, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.PurchaseOrderItemDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };

            revocations.PurchaseOrderItemReturnRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Return),
            };

            var writePermissions = new List<Permission>();
            foreach (var roleType in this.Meta.DatabaseRoleTypes)
            {
                writePermissions.Add(permissions.Get(this.Meta, roleType, Operations.Write));
            }

            revocations.PurchaseOrderItemWriteRevocation.DeniedPermissions = writePermissions;

            var executePermissions = new List<Permission>();
            foreach (var methodType in this.Meta.MethodTypes)
            {
                if (!methodType.Equals(this.Meta.Return))
                {
                    executePermissions.Add(permissions.Get(this.Meta, methodType));
                }
            }

            revocations.PurchaseOrderItemExecuteRevocation.DeniedPermissions = executePermissions;
        }
    }
}
