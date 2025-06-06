// <copyright file="PurchaseOrderPaymentState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderPaymentState
    {
        public bool IsNotPaid => Equals(this.UniqueId, PurchaseOrderPaymentStates.NotPaidId);

        public bool IsPartiallyPaid => Equals(this.UniqueId, PurchaseOrderPaymentStates.PartiallyPaidId);

        public bool IsPaid => Equals(this.UniqueId, PurchaseOrderPaymentStates.PaidId);
    }
}
