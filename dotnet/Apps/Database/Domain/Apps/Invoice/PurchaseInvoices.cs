// <copyright file="PurchaseInvoices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class PurchaseInvoices
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.PurchaseInvoiceState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var created = new PurchaseInvoiceStates(this.Transaction).Created;
            var awaitingApproval = new PurchaseInvoiceStates(this.Transaction).AwaitingApproval;
            var notPaid = new PurchaseInvoiceStates(this.Transaction).NotPaid;
            var partiallyPaid = new PurchaseInvoiceStates(this.Transaction).PartiallyPaid;
            var paid = new PurchaseInvoiceStates(this.Transaction).Paid;
            var cancelled = new PurchaseInvoiceStates(this.Transaction).Cancelled;
            var rejected = new PurchaseInvoiceStates(this.Transaction).Rejected;
            var revising = new PurchaseInvoiceStates(this.Transaction).Revising;

            var approve = this.Meta.Approve;
            var reject = this.Meta.Reject;
            var confirm = this.Meta.Confirm;
            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var createSalesInvoice = this.Meta.CreateSalesInvoice;
            var delete = this.Meta.Delete;
            var setPaid = this.Meta.SetPaid;
            var revise = this.Meta.Revise;
            var finishRevising = this.Meta.FinishRevising;

            config.Deny(this.ObjectType, created, approve, reject, reopen, createSalesInvoice, setPaid, revise, finishRevising);
            config.Deny(this.ObjectType, cancelled, approve, reject, confirm, cancel, setPaid, createSalesInvoice, delete, revise, finishRevising);
            config.Deny(this.ObjectType, rejected, approve, reject, confirm, cancel, setPaid, createSalesInvoice, delete, revise, finishRevising);
            config.Deny(this.ObjectType, awaitingApproval, confirm, cancel, reopen, setPaid, delete, revise, finishRevising);
            config.Deny(this.ObjectType, notPaid, cancel, reject, approve, confirm, reopen, createSalesInvoice, delete, finishRevising);
            config.Deny(this.ObjectType, partiallyPaid, cancel, reject, approve, confirm, reopen, createSalesInvoice, delete, finishRevising);
            config.Deny(this.ObjectType, paid, cancel, reject, approve, confirm, reopen, createSalesInvoice, delete, finishRevising);

            if (revising != null)
            {
                config.Deny(this.ObjectType, revising, cancel, reject, approve, confirm, reopen, createSalesInvoice, delete, setPaid, revise);
            }

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
            };

            config.DenyExcept(this.ObjectType, notPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, partiallyPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, paid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
            config.DenyExcept(this.ObjectType, rejected, except, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.PurchaseInvoiceCreateSalesInvoiceRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.CreateSalesInvoice),
            };

            revocations.PurchaseInvoiceDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
