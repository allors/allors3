// <copyright file="ShipmentItemState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ShipmentItemState
    {
        public bool IsCreated => Equals(this.UniqueId, ShipmentItemStates.CreatedId);

        public bool IsPicked => Equals(this.UniqueId, ShipmentItemStates.PickedId);

        public bool IsPacked => Equals(this.UniqueId, ShipmentItemStates.PackedId);

        public bool IsShipped => Equals(this.UniqueId, ShipmentItemStates.ShippedId);
    }
}
