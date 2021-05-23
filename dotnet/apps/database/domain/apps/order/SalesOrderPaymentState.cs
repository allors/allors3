// <copyright file="SalesOrderPaymentState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderPaymentState
    {
        public bool IsNotPaid => Equals(this.UniqueId, SalesOrderPaymentStates.NotPaidId);

        public bool IsPartiallyPaid => Equals(this.UniqueId, SalesOrderPaymentStates.PartiallyPaidId);

        public bool IsPaid => Equals(this.UniqueId, SalesOrderPaymentStates.PaidId);
    }
}
