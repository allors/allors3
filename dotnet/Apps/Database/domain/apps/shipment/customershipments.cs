// <copyright file="CustomerShipments.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Allors.Database.Meta;

    public partial class CustomerShipments
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.ShipmentState);

        protected override void AppsSecure(Security config)
        {
            var created = new ShipmentStates(this.Transaction).Created;
            var picking = new ShipmentStates(this.Transaction).Picking;
            var picked = new ShipmentStates(this.Transaction).Picked;
            var packed = new ShipmentStates(this.Transaction).Packed;
            var shipped = new ShipmentStates(this.Transaction).Shipped;
            var cancelled = new ShipmentStates(this.Transaction).Cancelled;
            var onHold = new ShipmentStates(this.Transaction).OnHold;

            var pick = this.Meta.Pick;
            var setPacked = this.Meta.SetPacked;
            var hold = this.Meta.Hold;
            var @continue = this.Meta.Continue;
            var ship = this.Meta.Ship;
            var delete = this.Meta.Delete;

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
            };

            config.Deny(this.ObjectType, onHold, pick, setPacked, ship, hold, delete);
            config.Deny(this.ObjectType, created, setPacked, ship, @continue);
            config.Deny(this.ObjectType, picked, ship, pick, @continue, delete);
            config.Deny(this.ObjectType, packed, pick, @continue, delete);
            config.Deny(this.ObjectType, picking, pick, setPacked, ship, @continue, delete);

            config.Deny(this.ObjectType, cancelled, Operations.Execute, Operations.Write);
            config.DenyExcept(this.ObjectType, picking, except, Operations.Write);
            config.DenyExcept(this.ObjectType, picked, except, Operations.Write);
            config.DenyExcept(this.ObjectType, packed, except, Operations.Write);
            config.DenyExcept(this.ObjectType, onHold, except, Operations.Write);
            config.DenyExcept(this.ObjectType, shipped, except, Operations.Execute, Operations.Write);
        }
    }
}
