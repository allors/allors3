// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Allors.Meta;

    public class ShipmentPackageDerivation : DomainDerivation
    {
        public ShipmentPackageDerivation(M m) : base(m, new Guid("9CB50263-5EA0-4B16-85A2-117BEE8A570A")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.ShipmentPackage.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var shipmentPackage in matches.Cast<ShipmentPackage>())
            {
                var highestNumber = 0;
                if (shipmentPackage.ExistShipmentWhereShipmentPackage)
                {
                    foreach (ShipmentPackage shipmentPackageShipmentWhereShipmentPackage in shipmentPackage.ShipmentWhereShipmentPackage.ShipmentPackages)
                    {
                        if (shipmentPackageShipmentWhereShipmentPackage.ExistSequenceNumber && shipmentPackageShipmentWhereShipmentPackage.SequenceNumber > highestNumber)
                        {
                            highestNumber = shipmentPackageShipmentWhereShipmentPackage.SequenceNumber;
                        }
                    }

                    if (!shipmentPackage.ExistSequenceNumber || shipmentPackage.SequenceNumber == 0)
                    {
                        shipmentPackage.SequenceNumber = highestNumber + 1;
                    }
                }

                if (!shipmentPackage.ExistDocuments)
                {
                    var name =
                        $"Package {(shipmentPackage.ExistSequenceNumber ? shipmentPackage.SequenceNumber.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
                    shipmentPackage.AddDocument(new PackagingSlipBuilder(shipmentPackage.Strategy.Session).WithName(name).Build());
                }
            }
        }
    }
}
