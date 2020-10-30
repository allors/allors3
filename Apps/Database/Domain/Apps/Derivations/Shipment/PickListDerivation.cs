// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PickListDerivation : DomainDerivation
    {
        public PickListDerivation(M m) : base(m, new Guid("8D9F3C91-DBA7-44AA-AA60-C1A58CAFDF0D")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PickList.Class),
                new ChangedPattern(this.M.PickList.Picker),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickList>())
            {
                if (@this.Store.IsImmediatelyPicked)
                {
                    @this.SetPicked();

                    foreach (PickListItem pickListItem in @this.PickListItems)
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            var shipment = itemIssuance.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;
                            var package = shipment?.ShipmentPackages.FirstOrDefault();

                            if (@this.Store.AutoGenerateShipmentPackage
                                && package != null
                                && package.PackagingContents.FirstOrDefault(v => v.ShipmentItem.Equals(itemIssuance.ShipmentItem)) == null)
                            {
                                package.AddPackagingContent(
                                    new PackagingContentBuilder(@this.Strategy.Session)
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
