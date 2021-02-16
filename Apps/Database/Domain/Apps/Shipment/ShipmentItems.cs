// <copyright file="ShipmentItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors;

    public partial class ShipmentItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.ShipmentItemState);

        protected override void AppsSecure(Security config)
        {
            var created = new ShipmentItemStates(this.Transaction).Created;
            var picking = new ShipmentItemStates(this.Transaction).Picking;
            var picked = new ShipmentItemStates(this.Transaction).Picked;
            var packed = new ShipmentItemStates(this.Transaction).Packed;
            var shipped = new ShipmentItemStates(this.Transaction).Shipped;
            var received = new ShipmentItemStates(this.Transaction).Received;

            config.Deny(this.ObjectType, picking, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, picked, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, packed, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, shipped, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, received, Operations.Execute, Operations.Write);
        }
    }
}
