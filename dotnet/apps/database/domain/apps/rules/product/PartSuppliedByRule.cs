// <copyright file="PartSuppliedByDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class PartSuppliedByRule : Rule
    {
        public PartSuppliedByRule(MetaPopulation m) : base(m, new Guid("9bdcfcf1-3140-4e89-bea7-41b1662148b1")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart),
                m.SupplierOffering.RolePattern(v => v.FromDate, v => v.Part),
                m.SupplierOffering.RolePattern(v => v.ThroughDate, v => v.Part),
                m.SupplierOffering.RolePattern(v => v.AllVersions, v => v.AllVersions.SupplierOfferingVersion.Part),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Part>())
            {
                @this.RemoveSuppliedBy();
                foreach (var supplierOffering in @this.SupplierOfferingsWherePart)
                {
                    if (supplierOffering.FromDate <= @this.Transaction().Now()
                        && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= @this.Transaction().Now()))
                    {
                        @this.AddSuppliedBy(supplierOffering.Supplier);
                    }
                }
            }
        }
    }
}
