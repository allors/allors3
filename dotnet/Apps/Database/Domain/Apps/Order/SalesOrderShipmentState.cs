// <copyright file="SalesOrderShipmentState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderShipmentState
    {
        public bool IsNotApplicable => Equals(this.UniqueId, SalesOrderShipmentStates.NotApplicableId);

        public bool IsNotShipped => Equals(this.UniqueId, SalesOrderShipmentStates.NotShippedId);

        public bool IsPartiallyShipped => Equals(this.UniqueId, SalesOrderShipmentStates.PartiallyShippedId);

        public bool IsShipped => Equals(this.UniqueId, SalesOrderShipmentStates.ShippedId);

        public bool IsInProgress => Equals(this.UniqueId, SalesOrderShipmentStates.InProgressId);
    }
}
