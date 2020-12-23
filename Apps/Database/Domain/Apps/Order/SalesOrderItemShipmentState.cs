// <copyright file="SalesOrderItemShipmentState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderItemShipmentState
    {
        public bool IsNotShipped => Equals(this.UniqueId, SalesOrderItemShipmentStates.NotShippedId);

        public bool IsPartiallyShipped => Equals(this.UniqueId, SalesOrderItemShipmentStates.PartiallyShippedId);

        public bool IsShipped => Equals(this.UniqueId, SalesOrderItemShipmentStates.ShippedId);

        public bool IsInProgress => Equals(this.UniqueId, SalesOrderItemShipmentStates.InProgressId);
    }
}
