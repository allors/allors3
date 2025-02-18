// <copyright file="PurchaseReturns.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Allors.Database.Meta;

    public partial class PurchaseReturns
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.ShipmentState);

        protected override void AppsSecure(Security config)
        {
            var shipped = new ShipmentStates(this.Transaction).Shipped;
            var cancelled = new ShipmentStates(this.Transaction).Cancelled;

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
            };

            config.Deny(this.ObjectType, cancelled, Operations.Execute, Operations.Write);
            config.DenyExcept(this.ObjectType, shipped, except, Operations.Execute, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.PurchaseReturnShipRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Ship),
            };
        }
    }
}
