// <copyright file="PurchaseOrderItemShipmentState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderItemShipmentState
    {
        public bool IsNotReceived => Equals(this.UniqueId, PurchaseOrderItemShipmentStates.NotReceivedId);

        public bool IsPartiallyReceived => Equals(this.UniqueId, PurchaseOrderItemShipmentStates.PartiallyReceivedId);

        public bool IsReceived => Equals(this.UniqueId, PurchaseOrderItemShipmentStates.ReceivedId);

        public bool IsReturned => Equals(this.UniqueId, PurchaseOrderItemShipmentStates.ReturnedId);

        public bool IsNa => Equals(this.UniqueId, PurchaseOrderItemShipmentStates.NaId);
    }
}
