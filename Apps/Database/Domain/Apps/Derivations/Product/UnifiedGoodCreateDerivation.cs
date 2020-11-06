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

    public class UnifiedGoodCreateDerivation : DomainDerivation
    {
        public UnifiedGoodCreateDerivation(M m) : base(m, new Guid("996c5b2f-3f38-49c3-a4b9-e63662a79b3c")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.UnifiedGood.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<UnifiedGood>())
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
            }
        }
    }
}
