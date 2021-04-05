// <copyright file="CountryVatRegimesDerivation.cs" company="Allors bvba">
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

    public class CountryVatRegimesRule : Rule
    {
        public CountryVatRegimesRule(MetaPopulation m) : base(m, new Guid("ac33cb9d-f694-4247-ad5d-fdae01d05c07")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.VatRegime.Country),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Country>())
            {

                foreach (VatRegime vatRegime in @this.VatRegimesWhereCountry)
                {
                    var previousCountry = vatRegime.CurrentVersion?.Country;
                    if (previousCountry != null && previousCountry != vatRegime.Country)
                    {
                        previousCountry.RemoveDerivedVatRegime(vatRegime);
                    }

                    @this.AddDerivedVatRegime(vatRegime);
                }
            }
        }
    }
}
