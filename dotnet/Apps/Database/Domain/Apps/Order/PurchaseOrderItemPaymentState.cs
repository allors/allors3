// <copyright file="PurchaseOrderItemPaymentState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderItemPaymentState
    {
        public bool IsNotPaid => Equals(this.UniqueId, PurchaseOrderItemPaymentStates.NotPaidId);

        public bool IsPartiallyPaid => Equals(this.UniqueId, PurchaseOrderItemPaymentStates.PartiallyPaidId);

        public bool IsPaid => Equals(this.UniqueId, PurchaseOrderItemPaymentStates.PaidId);
    }
}
