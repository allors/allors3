// <copyright file="SalesOrderShipmentState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderShipmentState
    {
        public bool IsNotShipped => Equals(this.UniqueId, SalesOrderShipmentStates.NotShippedId);

        public bool IsPartiallyShipped => Equals(this.UniqueId, SalesOrderShipmentStates.PartiallyShippedId);

        public bool IsShipped => Equals(this.UniqueId, SalesOrderShipmentStates.ShippedId);

        public bool IsInProgress => Equals(this.UniqueId, SalesOrderShipmentStates.InProgressId);
    }
}
