// <copyright file="OrderItemBilling.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class OrderItemBilling
    {
        public void AppsDelete(OrderItemBillingDelete method) => this.OrderItem.DerivationTrigger = Guid.NewGuid();
    }
}
