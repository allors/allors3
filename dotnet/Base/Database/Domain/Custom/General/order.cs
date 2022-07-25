// <copyright file="Order.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the HomeAddress type.</summary>

namespace Allors.Database.Domain
{
    public partial class Order
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations =>
            new[]
            {
                new TransitionalConfiguration(this.M.Order, this.M.Order.OrderState),
                new TransitionalConfiguration(this.M.Order, this.M.Order.ShipmentState),
                new TransitionalConfiguration(this.M.Order, this.M.Order.PaymentState),
            };
    }
}
