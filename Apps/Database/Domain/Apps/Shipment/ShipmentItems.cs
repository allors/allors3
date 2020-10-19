// <copyright file="ShipmentItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors;

    public partial class ShipmentItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.ShipmentItemState);

        protected override void AppsSecure(Security config)
        {
            var created = new ShipmentItemStates(this.Session).Created;
            var picking = new ShipmentItemStates(this.Session).Picking;
            var picked = new ShipmentItemStates(this.Session).Picked;
            var packed = new ShipmentItemStates(this.Session).Packed;
            var shipped = new ShipmentItemStates(this.Session).Shipped;
            var received = new ShipmentItemStates(this.Session).Received;

            config.Deny(this.ObjectType, picking, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, picked, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, packed, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, shipped, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, received, Operations.Execute, Operations.Write);
        }
    }
}