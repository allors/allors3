// <copyright file="SalesInvoiceItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Allors;
    using Allors.Database.Meta;

    public partial class SalesInvoiceItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesInvoiceItemState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var readyForPosting = new SalesInvoiceItemStates(this.Transaction).ReadyForPosting;
            var notPaid = new SalesInvoiceItemStates(this.Transaction).NotPaid;
            var paid = new SalesInvoiceItemStates(this.Transaction).Paid;
            var writtenOff = new SalesInvoiceItemStates(this.Transaction).WrittenOff;
            var cancelledByInvoice = new SalesInvoiceItemStates(this.Transaction).CancelledByInvoice;

            var except = new HashSet<IOperandType>
            {
                this.Meta.InternalComment,
                this.Meta.Comment,
                this.Meta.Description,
                this.Meta.Message,
            };

            config.DenyExcept(this.ObjectType, notPaid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, paid, except, Operations.Write);
            config.DenyExcept(this.ObjectType, writtenOff, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelledByInvoice, except, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.SalesInvoiceItemDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
