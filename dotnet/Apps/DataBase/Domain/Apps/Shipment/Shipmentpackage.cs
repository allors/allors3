// <copyright file="ShipmentPackage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ShipmentPackage
    {
        public decimal TotalQuantity
        {
            get
            {
                var total = 0M;
                foreach (var packagingContent in this.PackagingContents)
                {
                    total += packagingContent.Quantity;
                }

                return total;
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCreationDate)
            {
                this.CreationDate = this.Transaction().Now();
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var highestNumber = 0;
            if (this.ExistShipmentWhereShipmentPackage)
            {
                foreach (var shipmentPackageShipmentWhereShipmentPackage in this.ShipmentWhereShipmentPackage.ShipmentPackages)
                {
                    if (shipmentPackageShipmentWhereShipmentPackage.ExistSequenceNumber && shipmentPackageShipmentWhereShipmentPackage.SequenceNumber > highestNumber)
                    {
                        highestNumber = shipmentPackageShipmentWhereShipmentPackage.SequenceNumber;
                    }
                }

                this.SequenceNumber = highestNumber + 1;
            }
        }
    }
}
