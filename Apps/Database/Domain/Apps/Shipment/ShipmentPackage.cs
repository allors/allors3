// <copyright file="ShipmentPackage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class ShipmentPackage
    {
        public decimal TotalQuantity
        {
            get
            {
                var total = 0M;
                foreach (PackagingContent packagingContent in this.PackagingContents)
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
                this.CreationDate = this.Session().Now();
            }
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            //var derivation = method.Derivation;

            //this.AppsOnDeriveSequenceNumber(derivation);

            //if (!this.ExistDocuments)
            //{
            //    var name =
            //        $"Package {(this.ExistSequenceNumber ? this.SequenceNumber.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
            //    this.AddDocument(new PackagingSlipBuilder(this.Strategy.Session).WithName(name).Build());
            //}
        }

        //public void AppsOnDeriveSequenceNumber(IDerivation derivation)
        //{
        //    var highestNumber = 0;
        //    if (this.ExistShipmentWhereShipmentPackage)
        //    {
        //        foreach (ShipmentPackage shipmentPackage in this.ShipmentWhereShipmentPackage.ShipmentPackages)
        //        {
        //            if (shipmentPackage.ExistSequenceNumber && shipmentPackage.SequenceNumber > highestNumber)
        //            {
        //                highestNumber = shipmentPackage.SequenceNumber;
        //            }
        //        }

        //        if (!this.ExistSequenceNumber || this.SequenceNumber == 0)
        //        {
        //            this.SequenceNumber = highestNumber + 1;
        //        }
        //    }
        //}
    }
}
