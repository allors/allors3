// <copyright file="PurchaseOrderItemPaymentStates.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseOrderItemPaymentStates
    {
        internal static readonly Guid NotPaidId = new Guid("CAA6FCD8-3350-4113-945D-4C6A6398F317");
        internal static readonly Guid PaidId = new Guid("25AC349F-ECCD-4AB0-B032-0E5F14618083");
        internal static readonly Guid PartiallyPaidId = new Guid("B4269B24-DBCF-4548-A559-BC3D7A617D97");

        private UniquelyIdentifiableCache<PurchaseOrderItemPaymentState> cache;

        public PurchaseOrderItemPaymentState NotPaid => this.Cache[NotPaidId];

        public PurchaseOrderItemPaymentState Paid => this.Cache[PaidId];

        public PurchaseOrderItemPaymentState PartiallyPaid => this.Cache[PartiallyPaidId];

        private UniquelyIdentifiableCache<PurchaseOrderItemPaymentState> Cache => this.cache ??= new UniquelyIdentifiableCache<PurchaseOrderItemPaymentState>(this.Session);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(NotPaidId, v => v.Name = "Not Paid");
            merge(PartiallyPaidId, v => v.Name = "Partially Paid");
            merge(PaidId, v => v.Name = "Paid");
        }
    }
}
