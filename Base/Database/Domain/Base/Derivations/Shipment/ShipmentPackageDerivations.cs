// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class ShipmentPackageCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdShipmentPackage = changeSet.Created.Select(session.Instantiate).OfType<ShipmentPackage>();

                foreach(var shipmentPackage in createdShipmentPackage)
                {
                    BaseOnDeriveSequenceNumber(shipmentPackage);
                    
                    if (!shipmentPackage.ExistDocuments)
                    {
                        var name =
                            $"Package {(shipmentPackage.ExistSequenceNumber ? shipmentPackage.SequenceNumber.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
                        shipmentPackage.AddDocument(new PackagingSlipBuilder(shipmentPackage.Strategy.Session).WithName(name).Build());
                    }
                }

                void BaseOnDeriveSequenceNumber(ShipmentPackage shipmentPackage)
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
                }
            }

            
        }

        public static void ShipmentPackageRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("5fa705d2-5170-48bc-943c-76c45781e365")] = new TransferCreationDerivation();
        }
    }
}
