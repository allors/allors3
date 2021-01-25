// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class NonUnifiedPartSuppliedByDerivation : DomainDerivation
    {
        public NonUnifiedPartSuppliedByDerivation(M m) : base(m, new Guid("9bdcfcf1-3140-4e89-bea7-41b1662148b1")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SupplierOffering.Part) { Steps = new IPropertyType[]{ m.SupplierOffering.Part }, OfType = m.NonUnifiedPart.Class},
                new ChangedPattern(m.SupplierOffering.FromDate) { Steps = new IPropertyType[]{ m.SupplierOffering.Part }, OfType = m.NonUnifiedPart.Class},
                new ChangedPattern(m.SupplierOffering.ThroughDate) { Steps = new IPropertyType[]{ m.SupplierOffering.Part }, OfType = m.NonUnifiedPart.Class},
                new ChangedPattern(m.SupplierOffering.AllVersions) { Steps = new IPropertyType[]{ m.SupplierOffering.AllVersions, m.SupplierOfferingVersion.Part }, OfType = m.NonUnifiedPart.Class},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                @this.RemoveSuppliedBy();
                foreach (SupplierOffering supplierOffering in @this.SupplierOfferingsWherePart)
                {
                    if (supplierOffering.FromDate <= @this.Session().Now()
                        && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= @this.Session().Now()))
                    {
                        @this.AddSuppliedBy(supplierOffering.Supplier);
                    }
                }
            }
        }
    }
}
