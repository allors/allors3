// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PickListCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPickLists = changeSet.Created.Select(session.Instantiate).OfType<PickList>();

                foreach(var pickLists in createdPickLists)
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

        public static void PickListRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0a544cd5-a663-460f-9850-ca44480bb096")] = new PickListCreationDerivation();
        }
    }
}
