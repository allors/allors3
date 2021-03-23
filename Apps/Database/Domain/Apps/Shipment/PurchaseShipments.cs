// <copyright file="PurchaseShipments.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Allors.Database.Meta;

    public partial class PurchaseShipments
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.ShipmentState);

        protected override void AppsSecure(Security config)
        {
            var received = new ShipmentStates(this.Transaction).Received;
            var cancelled = new ShipmentStates(this.Transaction).Cancelled;

            var except = new HashSet<OperandType>
            {
                this.Meta.ElectronicDocuments,
            };

            config.Deny(this.ObjectType, cancelled, Operations.Execute, Operations.Write);
            config.DenyExcept(this.ObjectType, received, except, Operations.Execute, Operations.Write);
        }
    }
}
