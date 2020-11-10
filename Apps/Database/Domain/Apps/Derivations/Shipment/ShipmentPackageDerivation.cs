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
                new ChangedPattern(this.M.ShipmentPackage.SequenceNumber),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ShipmentPackage>())
            {
                var highestNumber = 0;
                if (@this.ExistShipmentWhereShipmentPackage)
                {
                    foreach (ShipmentPackage shipmentPackageShipmentWhereShipmentPackage in @this.ShipmentWhereShipmentPackage.ShipmentPackages)
                    {
                        if (shipmentPackageShipmentWhereShipmentPackage.ExistSequenceNumber && shipmentPackageShipmentWhereShipmentPackage.SequenceNumber > highestNumber)
                        {
                            highestNumber = shipmentPackageShipmentWhereShipmentPackage.SequenceNumber;
                        }
                    }

                    if (!@this.ExistSequenceNumber || @this.SequenceNumber == 0)
                    {
                        @this.SequenceNumber = highestNumber + 1;
                    }
                }

                if (!@this.ExistDocuments)
                {
                    var name =
                        $"Package {(@this.ExistSequenceNumber ? @this.SequenceNumber.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
                    @this.AddDocument(new PackagingSlipBuilder(@this.Strategy.Session).WithName(name).Build());
                }
            }
        }
    }
}
