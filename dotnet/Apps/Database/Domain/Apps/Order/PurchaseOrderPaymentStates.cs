// <copyright file="PurchaseOrderPaymentStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseOrderPaymentStates
    {
        public static readonly Guid NotPaidId = new Guid("3DCF7D7D-4CD1-4D68-A909-7BEB672A2179");
        public static readonly Guid PaidId = new Guid("4BCF3FA8-5B30-482b-A762-2BF43721E045");
        public static readonly Guid PartiallyPaidId = new Guid("CB502944-27D9-4aad-9DAC-5D1F5A344D08");

        private UniquelyIdentifiableCache<PurchaseOrderPaymentState> cache;

        public PurchaseOrderPaymentState NotPaid => this.Cache[NotPaidId];

        public PurchaseOrderPaymentState Paid => this.Cache[PaidId];

        public PurchaseOrderPaymentState PartiallyPaid => this.Cache[PartiallyPaidId];

        private UniquelyIdentifiableCache<PurchaseOrderPaymentState> Cache => this.cache ??= new UniquelyIdentifiableCache<PurchaseOrderPaymentState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(NotPaidId, v => v.Name = "Not Paid");
            merge(PaidId, v => v.Name = "Paid");
            merge(PartiallyPaidId, v => v.Name = "Partially Paid");
        }
    }
}
