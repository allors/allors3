// <copyright file="CountryVatRegimesDerivation.cs" company="Allors bvba">
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

    public class CountryVatRegimesRule : Rule
    {
        public CountryVatRegimesRule(MetaPopulation m) : base(m, new Guid("ac33cb9d-f694-4247-ad5d-fdae01d05c07")) =>
            this.Patterns = new Pattern[]
            {
                m.Country.AssociationPattern(v => v.VatRegimesWhereCountry),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Country>())
            {
                foreach (var vatRegime in @this.VatRegimesWhereCountry)
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
