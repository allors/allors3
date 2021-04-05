// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class ShipmentPackageRule : Rule
    {
        public ShipmentPackageRule(MetaPopulation m) : base(m, new Guid("9CB50263-5EA0-4B16-85A2-117BEE8A570A")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.ShipmentPackage, m.ShipmentPackage.SequenceNumber),
                new RolePattern(m.PickList, m.PickList.PickListState) { Steps = new IPropertyType[] { m.PickList.PickListItems, m.PickListItem.ItemIssuancesWherePickListItem, m.ItemIssuance.ShipmentItem, m.ShipmentItem.ShipmentWhereShipmentItem, m.Shipment.ShipmentPackages } },
                new AssociationPattern(m.ItemIssuance.ShipmentItem) { Steps = new IPropertyType[] { m.ShipmentItem.ShipmentWhereShipmentItem, m.Shipment.ShipmentPackages } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ShipmentPackage>())
            {
                if (!@this.ExistDocuments)
                {
                    var name = $"Package {(@this.ExistSequenceNumber ? @this.SequenceNumber.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
                    @this.AddDocument(new PackagingSlipBuilder(@this.Strategy.Transaction).WithName(name).Build());
                }

                var shipment = @this.ShipmentWhereShipmentPackage as CustomerShipment;

                if (shipment!= null && shipment.Store.AutoGenerateShipmentPackage)
                {
                    foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
                    {
                        foreach (ItemIssuance itemIssuance in shipmentItem.ItemIssuancesWhereShipmentItem)
                        {
                            if (itemIssuance.PickListItem.PickListWherePickListItem.PickListState.IsPicked
                                && @this.PackagingContents.FirstOrDefault(v => v.ShipmentItem.Equals(itemIssuance.ShipmentItem)) == null)
                            {
                                @this.AddPackagingContent(
                                    new PackagingContentBuilder(@this.Strategy.Transaction)
                                        .WithShipmentItem(itemIssuance.ShipmentItem)
                                        .WithQuantity(itemIssuance.Quantity)
                                        .Build());
                            }
                        }
                    }
                }
            }
        }
    }
}
