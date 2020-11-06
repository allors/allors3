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

    public class InventoryItemExtensionsCreateDerivation : DomainDerivation
    {
        public InventoryItemExtensionsCreateDerivation(M m) : base(m, new Guid("02b1e341-e887-4a64-948c-ccdc001a3fc2")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.InventoryItem.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<InventoryItem>())
            {
                // TODO: Let Sync set Unit of Measure
                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = @this.Part?.UnitOfMeasure;
                }
            }
        }
    }
}
