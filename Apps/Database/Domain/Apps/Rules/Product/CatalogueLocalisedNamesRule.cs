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
    using Database.Derivations;

    public class CatalogueLocalisedNamesRule : Rule
    {
        public CatalogueLocalisedNamesRule(MetaPopulation m) : base(m, new Guid("4bd61c11-e431-4355-a729-66659608ca01")) =>
            this.Patterns = new Pattern[]
            {
                m.Catalogue.RolePattern(v => v.LocalisedNames),
                m.LocalisedText.RolePattern(v => v.Text, v => v.CatalogueWhereLocalisedName),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Catalogue>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
