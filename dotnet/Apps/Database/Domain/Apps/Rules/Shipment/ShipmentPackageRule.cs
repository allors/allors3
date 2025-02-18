// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ShipmentPackageRule : Rule
    {
        public ShipmentPackageRule(MetaPopulation m) : base(m, new Guid("9CB50263-5EA0-4B16-85A2-117BEE8A570A")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentPackage.RolePattern(v => v.SequenceNumber),
                m.PickList.RolePattern(v => v.PickListState, v => v.PickListItems.ObjectType.ItemIssuancesWherePickListItem.ObjectType.ShipmentItem.ObjectType.ShipmentWhereShipmentItem.ObjectType.ShipmentPackages),
                m.ShipmentItem.AssociationPattern(v => v.ItemIssuancesWhereShipmentItem, v => v.ShipmentWhereShipmentItem.ObjectType.ShipmentPackages),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
                    foreach (var shipmentItem in shipment.ShipmentItems)
                    {
                        foreach (var itemIssuance in shipmentItem.ItemIssuancesWhereShipmentItem)
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
