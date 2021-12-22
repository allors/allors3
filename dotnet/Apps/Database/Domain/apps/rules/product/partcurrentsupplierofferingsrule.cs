// <copyright file="PartCurrentSupplierOfferingsNameRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PartCurrentSupplierOfferingsNameRule : Rule
    {
        public PartCurrentSupplierOfferingsNameRule(MetaPopulation m) : base(m, new Guid("b8942f8c-abd2-4fae-a1e0-6d9fcc312a02")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart),
                m.SupplierOffering.RolePattern(v => v.FromDate, v => v.Part),
                m.SupplierOffering.RolePattern(v => v.ThroughDate, v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Part>())
            {
                @this.DeriveRelationships();
            }
        }
    }
}
