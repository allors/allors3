// <copyright file="ShipmentItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class ShipmentItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.ShipmentItem, this.M.ShipmentItem.ShipmentItemState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistShipmentItemState)
            {
                this.ShipmentItemState = new ShipmentItemStates(this.Strategy.Transaction).Created;
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.ExistItemIssuancesWhereShipmentItem)
            {
                foreach (var itemIssuance in this.ItemIssuancesWhereShipmentItem)
                {
                    itemIssuance.Delete();
                }
            }
        }
    }
}
