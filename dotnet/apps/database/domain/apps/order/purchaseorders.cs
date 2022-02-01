// <copyright file="PurchaseOrders.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public partial class PurchaseOrders
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.PurchaseOrderState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var states = new PurchaseOrderStates(this.Transaction);
            var created = states.Created;
            var onHold = states.OnHold;
            var cancelled = states.Cancelled;
            var rejected = states.Rejected;
            var awaitingApprovalLevel1 = states.AwaitingApprovalLevel1;
            var awaitingApprovalLevel2 = states.AwaitingApprovalLevel2;
            var inProcess = states.InProcess;
            var sent = states.Sent;
            var completed = states.Completed;
            var finished = states.Finished;

            var approve = this.Meta.Approve;
            var reject = this.Meta.Reject;
            var hold = this.Meta.Hold;
            var @continue = this.Meta.Continue;
            var setReadyForProcessing = this.Meta.SetReadyForProcessing;
            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var send = this.Meta.Send;
            var revise = this.Meta.Revise;
            var quickReceive = this.Meta.QuickReceive;
            var @return = this.Meta.Return;
            var invoice = this.Meta.Invoice;

            config.Deny(this.ObjectType, created, approve, cancel, reject, @continue, reopen, send, quickReceive, invoice, revise, @return);
            config.Deny(this.ObjectType, onHold, approve, hold, setReadyForProcessing, reopen, send, quickReceive, invoice, revise, @return);
            config.Deny(this.ObjectType, cancelled, approve, reject, hold, @continue, setReadyForProcessing, cancel, send, quickReceive, invoice, revise, @return);
            config.Deny(this.ObjectType, rejected, approve, reject, hold, @continue, setReadyForProcessing, cancel, send, quickReceive, invoice, revise, @return);
            config.Deny(this.ObjectType, awaitingApprovalLevel1, hold, @continue, setReadyForProcessing, cancel, reopen, send, quickReceive, @continue, revise, @return);
            config.Deny(this.ObjectType, awaitingApprovalLevel2, hold, @continue, setReadyForProcessing, cancel, reopen, send, quickReceive, @continue, revise, @return);
            config.Deny(this.ObjectType, inProcess, approve, reject, hold, @continue, setReadyForProcessing, reopen, quickReceive, @return);
            config.Deny(this.ObjectType, sent, approve, reject, hold, @continue, setReadyForProcessing, reopen, send, @return);
            config.Deny(this.ObjectType, completed, approve, reject, hold, @continue, setReadyForProcessing, cancel, reopen, send, quickReceive);

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
            };

            config.DenyExcept(this.ObjectType, inProcess, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
            config.DenyExcept(this.ObjectType, rejected, except, Operations.Write);

            var exceptCompleted = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
                this.Meta.Return,
            };

            config.DenyExcept(this.ObjectType, completed, exceptCompleted, Operations.Write);
            config.DenyExcept(this.ObjectType, finished, exceptCompleted, Operations.Execute, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.PurchaseOrderDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };

            revocations.PurchaseOrderInvoiceRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Invoice),
            };

            revocations.PurchaseOrderQuickReceiveRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.QuickReceive),
            };

            revocations.PurchaseOrderReturnRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Return),
            };

            revocations.PurchaseOrderReviseRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Revise),
            };

            revocations.PurchaseOrderReceivedRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Cancel),
                permissions.Get(this.Meta, this.Meta.Reject),
                permissions.Get(this.Meta, this.Meta.QuickReceive),
                permissions.Get(this.Meta, this.Meta.Revise),
                permissions.Get(this.Meta, this.Meta.SetReadyForProcessing),
            };

            revocations.PurchaseOrderReopenRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Reopen),
            };

            var writePermissions = new List<Permission>();
            foreach (var roleType in this.Meta.DatabaseRoleTypes)
            {
                writePermissions.Add(permissions.Get(this.Meta, roleType, Operations.Write));
            }

            revocations.PurchaseOrderWriteRevocation.DeniedPermissions = writePermissions;
        }
    }
}
