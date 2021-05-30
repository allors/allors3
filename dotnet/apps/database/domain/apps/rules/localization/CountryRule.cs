// <copyright file="Domain.cs" company="Allors bvba">
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

    public class CountryRule : Rule
    {
        public CountryRule(MetaPopulation m) : base(m, new Guid("9cc7cee8-40b2-48bd-9f37-78d5fe86cd07")) =>
            this.Patterns = new Pattern[]
            {
                m.Country.RolePattern(v => v.IsoCode),
                m.Country.AssociationPattern(v => v.VatRegimesWhereCountry),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Country>())
            {
                if (@this.ExistIsoCode)
                {
                    @this.EuMemberState = Countries.EuMemberStates.Contains(@this.IsoCode);

                    if (Countries.IbanDataByCountry.TryGetValue(@this.IsoCode, out var ibanData))
                    {
                        @this.IbanLength = ibanData.Length;
                        @this.IbanRegex = ibanData.RegexStructure;
                    }
                    else
                    {
                        @this.RemoveIbanLength();
                        @this.RemoveIbanRegex();
                    }
                }
                else
                {
                    @this.RemoveEuMemberState();
                    @this.RemoveIbanLength();
                    @this.RemoveIbanRegex();
                }
            }
        }
    }
}
