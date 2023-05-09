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

    public class CountryDerivedVatRegimesRule : Rule
    {
        public CountryDerivedVatRegimesRule(MetaPopulation m) : base(m, new Guid("ac33cb9d-f694-4247-ad5d-fdae01d05c07")) =>
            this.Patterns = new Pattern[]
            {
                m.Country.AssociationPattern(v => v.VatRegimesWhereCountry),
                m.VatRegime.RolePattern(v => v.Countries, v => v.Countries),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Country>())
            {
                @this.DeriveCountryDerivedVatRegimes(validation);
            }
        }
    }

    public static class CountryDerivedVatRegimesRuleExtensions
    {
        public static void DeriveCountryDerivedVatRegimes(this Country @this, IValidation validation)
        {
            @this.RemoveDerivedVatRegimes();

            foreach (var vatRegime in @this.VatRegimesWhereCountry)
            {
                @this.AddDerivedVatRegime(vatRegime);
            }
        }
    }
}
