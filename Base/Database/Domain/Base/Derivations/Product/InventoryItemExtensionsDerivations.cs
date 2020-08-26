// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class InventoryItemExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdInventoryItemExtensions = changeSet.Created.Select(session.Instantiate).OfType<InventoryItem>();

                foreach(var inventoryItemExtensions in createdInventoryItemExtensions)
                {
                    var now = session.Now();

                    (inventoryItemExtensions).PartDisplayName = inventoryItemExtensions.Part?.DisplayName;

                    if (!inventoryItemExtensions.ExistFacility && inventoryItemExtensions.ExistPart && inventoryItemExtensions.Part.ExistDefaultFacility)
                    {
                        inventoryItemExtensions.Facility = inventoryItemExtensions.Part.DefaultFacility;
                    }

                    // TODO: Let Sync set Unit of Measure
                    if (!inventoryItemExtensions.ExistUnitOfMeasure)
                    {
                        inventoryItemExtensions.UnitOfMeasure = inventoryItemExtensions.Part?.UnitOfMeasure;
                    }

                    var part = inventoryItemExtensions.Part;

                    var builder = new StringBuilder();

                    builder.Append(part.SearchString);

                    inventoryItemExtensions.SearchString = builder.ToString();
                }
               
            }

        }

        public static void InventoryItemExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("02dd4166-777c-48a6-8b4b-de8cff5e2468")] = new InventoryItemExtensionsCreationDerivation();
        }
    }
}
