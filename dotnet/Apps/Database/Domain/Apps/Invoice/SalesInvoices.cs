
// <copyright file="SalesInvoices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class SalesInvoices
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesInvoiceState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var notPaid = new SalesInvoiceStates(this.Transaction).NotPaid;
            var paid = new SalesInvoiceStates(this.Transaction).Paid;
            var partiallyPaid = new SalesInvoiceStates(this.Transaction).PartiallyPaid;
            var writtenOff = new SalesInvoiceStates(this.Transaction).WrittenOff;
            var cancelled = new SalesInvoiceStates(this.Transaction).Cancelled;
            var readyForPosting = new SalesInvoiceStates(this.Transaction).ReadyForPosting;

            var send = this.Meta.Send;
            var cancelInvoice = this.Meta.CancelInvoice;
            var writeOff = this.Meta.WriteOff;
            var reopen = this.Meta.Reopen;
            var credit = this.Meta.Credit;
            var setPaid = this.Meta.SetPaid;
            var delete = this.Meta.Delete;
            var revise = this.Meta.Revise;

            config.Deny(this.ObjectType, readyForPosting, reopen, credit, setPaid, revise, writeOff);
            config.Deny(this.ObjectType, notPaid, send, reopen, delete); //cancelInvoice and revise is allowed for creditnote
            config.Deny(this.ObjectType, partiallyPaid, send, cancelInvoice, reopen, revise, delete);
            config.Deny(this.ObjectType, paid, send, writeOff, cancelInvoice, reopen, setPaid, revise, delete);
            config.Deny(this.ObjectType, writtenOff, send, cancelInvoice, writeOff, credit, setPaid, delete, revise);
            config.Deny(this.ObjectType, cancelled, send, cancelInvoice, writeOff, credit, setPaid, revise, delete);

            var except = new HashSet<IOperandType>
            {
                this.Meta.InternalComment,
                this.Meta.Comment,
                this.Meta.Description,
                this.Meta.Message,
                this.Meta.InvoiceDate,
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
                this.Meta.Credit,
                this.Meta.AssignedBillToContactMechanism,
                this.Meta.AssignedBillToEndCustomerContactMechanism,
                this.Meta.AssignedShipToAddress,
                this.Meta.AssignedShipToEndCustomerAddress,
                this.Meta.BillToContactPerson,
                this.Meta.ShipToContactPerson,
            };

            config.DenyExcept(this.ObjectType, notPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, partiallyPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, paid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, writtenOff, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.SalesInvoiceDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };

            revocations.SalesInvoiceCancelRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.CancelInvoice),
            };

            revocations.SalesInvoiceReviseRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Revise),
            };
        }
    }
}
