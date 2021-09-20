// <copyright file="SalesInvoiceItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors;

    public partial class SalesInvoiceItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesInvoiceItemState);

        protected override void AppsSecure(Security config)
        {
            var readyForPosting = new SalesInvoiceItemStates(this.Transaction).ReadyForPosting;
            var notPaid = new SalesInvoiceItemStates(this.Transaction).NotPaid;
            var paid = new SalesInvoiceItemStates(this.Transaction).Paid;
            var writtenOff = new SalesInvoiceItemStates(this.Transaction).WrittenOff;
            var cancelledByInvoice = new SalesInvoiceItemStates(this.Transaction).CancelledByInvoice;

            config.Deny(this.ObjectType, notPaid, Operations.Write);
            config.Deny(this.ObjectType, paid, Operations.Write);
            config.Deny(this.ObjectType, writtenOff, Operations.Write);
            config.Deny(this.ObjectType, cancelledByInvoice, Operations.Write);
        }
    }
}
