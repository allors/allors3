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

    public class NonUnifiedPartCreateDerivation : DomainDerivation
    {
        public NonUnifiedPartCreateDerivation(M m) : base(m, new Guid("1d63e869-ec54-4d81-b62c-bd0dca787a4b")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.NonUnifiedPart.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                if (!@this.ExistInventoryItemKind)
                {
                    @this.InventoryItemKind = new InventoryItemKinds(@this.Strategy.Session).NonSerialised;
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Session).Piece;
                }

                if (!@this.ExistDefaultFacility)
                {
                    @this.DefaultFacility = @this.Strategy.Session.GetSingleton().Settings.DefaultFacility;
                }

                if (!@this.ExistName)
                {
                    @this.Name = "Part " + (@this.PartIdentification() ?? @this.UniqueId.ToString());
                }
            }
        }
    }
}
