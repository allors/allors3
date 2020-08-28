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
        public class PackagingContentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPackagingContents = changeSet.Created.Select(session.Instantiate).OfType<PackagingContent>();

                foreach (var packagingContent in createdPackagingContents)
                {
                    if (packagingContent.ExistQuantity && packagingContent.ExistShipmentItem)
                    {
                        var maxQuantity = packagingContent.ShipmentItem.Quantity - packagingContent.ShipmentItem.QuantityShipped;
                        if (packagingContent.Quantity == 0 || packagingContent.Quantity > maxQuantity)
                        {
                            validation.AddError($"{packagingContent} {M.PackagingContent.Quantity} {ErrorMessages.PackagingContentMaximum}");
                        }
                    }
                }
            }
        }

        public static void PackagingContentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("cdbd9914-b5f4-481d-bdc4-e2cbed8b2d47")] = new PackagingContentCreationDerivation();
        }
    }
}
