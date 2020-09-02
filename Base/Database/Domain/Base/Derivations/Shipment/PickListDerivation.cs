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

    public class PickListDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("8D9F3C91-DBA7-44AA-AA60-C1A58CAFDF0D");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.PickList.Class),
            new ChangedRolePattern(M.PickList.Picker),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var pickLists in matches.Cast<PickList>())
            {
                if (pickLists.Store.IsImmediatelyPicked)
                {
                    pickLists.SetPicked();

                    foreach (PickListItem pickListItem in pickLists.PickListItems)
                    {
                        foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            var shipment = itemIssuance.ShipmentItem.ShipmentWhereShipmentItem as CustomerShipment;
                            var package = shipment?.ShipmentPackages.FirstOrDefault();

                            if (pickLists.Store.AutoGenerateShipmentPackage
                                && package != null
                                && package.PackagingContents.FirstOrDefault(v => v.ShipmentItem.Equals(itemIssuance.ShipmentItem)) == null)
                            {
                                package.AddPackagingContent(
                                    new PackagingContentBuilder(pickLists.Strategy.Session)
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
