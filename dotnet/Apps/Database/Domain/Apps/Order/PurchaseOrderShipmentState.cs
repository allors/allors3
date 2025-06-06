// <copyright file="PurchaseOrderShipmentState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderShipmentState
    {
        public bool IsNotReceived => Equals(this.UniqueId, PurchaseOrderShipmentStates.NotReceivedId);

        public bool IsPartiallyReceived => Equals(this.UniqueId, PurchaseOrderShipmentStates.PartiallyReceivedId);

        public bool IsReceived => Equals(this.UniqueId, PurchaseOrderShipmentStates.ReceivedId);

        public bool IsNa => Equals(this.UniqueId, PurchaseOrderShipmentStates.NaId);
    }
}
