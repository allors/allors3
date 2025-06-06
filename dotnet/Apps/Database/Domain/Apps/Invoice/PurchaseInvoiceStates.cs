// <copyright file="PurchaseInvoiceStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseInvoiceStates
    {
        public static readonly Guid CreatedId = new Guid("102F4080-1D12-4090-9196-F42C094C38CA");
        public static readonly Guid AwaitingApprovalId = new Guid("FE3A30A9-0174-4534-A11E-E772112E9760");
        public static readonly Guid PartiallyPaidId = new Guid("9D917078-7ACD-4F04-AE6B-24E33755E9B1");
        public static readonly Guid NotPaidId = new Guid("3D811CFE-3EC0-4975-80B8-012A42B2B3E2");
        public static readonly Guid PaidId = new Guid("2982C8BE-657E-4594-BCAF-98997AFEA9F8");
        public static readonly Guid CancelledId = new Guid("60650051-F1F1-4dd6-90C8-5E744093D2EE");
        public static readonly Guid RejectedId = new Guid("26E27DDC-0782-4C29-B4BE-FF1E7AEE788A");
        public static readonly Guid RevisingId = new Guid("639ba038-d8f3-4672-80b5-c8eb96e3275d");

        private UniquelyIdentifiableCache<PurchaseInvoiceState> cache;

        public PurchaseInvoiceState Created => this.Cache[CreatedId];

        public PurchaseInvoiceState AwaitingApproval => this.Cache[AwaitingApprovalId];

        public PurchaseInvoiceState PartiallyPaid => this.Cache[PartiallyPaidId];

        public PurchaseInvoiceState NotPaid => this.Cache[NotPaidId];

        public PurchaseInvoiceState Paid => this.Cache[PaidId];

        public PurchaseInvoiceState Cancelled => this.Cache[CancelledId];

        public PurchaseInvoiceState Rejected => this.Cache[RejectedId];

        public PurchaseInvoiceState Revising => this.Cache[RevisingId];

        private UniquelyIdentifiableCache<PurchaseInvoiceState> Cache => this.cache ??= new UniquelyIdentifiableCache<PurchaseInvoiceState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(CreatedId, v => v.Name = "Created");
            merge(AwaitingApprovalId, v => v.Name = "Awaiting Approval");
            merge(PartiallyPaidId, v => v.Name = "Partially Paid");
            merge(NotPaidId, v => v.Name = "Not Paid");
            merge(PaidId, v => v.Name = "Paid");
            merge(CancelledId, v => v.Name = "Cancelled");
            merge(RejectedId, v => v.Name = "Rejected");
            merge(RevisingId, v => v.Name = "Revising");
        }
    }
}
