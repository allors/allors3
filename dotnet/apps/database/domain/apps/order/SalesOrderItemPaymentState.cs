// <copyright file="SalesOrderItemPaymentState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderItemPaymentState
    {
        public bool IsNotPaid => Equals(this.UniqueId, SalesOrderItemPaymentStates.NotPaidId);

        public bool IsPartiallyPaid => Equals(this.UniqueId, SalesOrderItemPaymentStates.PartiallyPaidId);

        public bool IsPaid => Equals(this.UniqueId, SalesOrderItemPaymentStates.PaidId);
    }
}
