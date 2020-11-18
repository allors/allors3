
// <copyright file="SalesInvoices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Allors.Database.Meta;

    public partial class SalesInvoices
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesInvoiceState);

        protected override void AppsSecure(Security config)
        {
            var notPaid = new SalesInvoiceStates(this.Session).NotPaid;
            var paid = new SalesInvoiceStates(this.Session).Paid;
            var partiallyPaid = new SalesInvoiceStates(this.Session).PartiallyPaid;
            var writtenOff = new SalesInvoiceStates(this.Session).WrittenOff;
            var cancelled = new SalesInvoiceStates(this.Session).Cancelled;
            var readyForPosting = new SalesInvoiceStates(this.Session).ReadyForPosting;

            var send = this.Meta.Send;
            var cancelInvoice = this.Meta.CancelInvoice;
            var writeOff = this.Meta.WriteOff;
            var reopen = this.Meta.Reopen;
            var credit = this.Meta.Credit;
            var setPaid = this.Meta.SetPaid;
            var delete = this.Meta.Delete;
            var revise = this.Meta.Revise; // not implemented yet

            config.Deny(this.ObjectType, readyForPosting, reopen, credit, setPaid, revise, writeOff);
            config.Deny(this.ObjectType, notPaid, send, cancelInvoice, reopen, revise, delete);
            config.Deny(this.ObjectType, partiallyPaid, send, cancelInvoice, reopen, revise, delete);
            config.Deny(this.ObjectType, paid, send, writeOff, cancelInvoice, reopen, setPaid, revise, delete);
            config.Deny(this.ObjectType, writtenOff, send, cancelInvoice, writeOff, credit, setPaid, delete, revise, reopen);
            config.Deny(this.ObjectType, cancelled, send, cancelInvoice, writeOff, credit, setPaid, revise, delete);

            var except = new HashSet<OperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
                this.Meta.Credit,
            };

            config.DenyExcept(this.ObjectType, notPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, partiallyPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, paid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, writtenOff, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
        }
    }
}
