// <copyright file="PurchaseInvoiceItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseInvoiceItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.PurchaseInvoiceItemState);

        protected override void AppsSecure(Security config)
        {
            var created = new PurchaseInvoiceItemStates(this.Transaction).Created;
            var awaitingApproval = new PurchaseInvoiceItemStates(this.Transaction).AwaitingApproval;
            var paid = new PurchaseInvoiceItemStates(this.Transaction).Paid;
            var cancelledByinvoice = new PurchaseInvoiceItemStates(this.Transaction).CancelledByinvoice;
            var rejected = new PurchaseInvoiceItemStates(this.Transaction).Rejected;
            var revising = new PurchaseInvoiceItemStates(this.Transaction).Revising;

            var reject = this.Meta.Reject;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, created, reject);
            config.Deny(this.ObjectType, awaitingApproval, delete);

            config.Deny(this.ObjectType, cancelledByinvoice, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, paid, Operations.Execute, Operations.Write);
        }
    }
}
