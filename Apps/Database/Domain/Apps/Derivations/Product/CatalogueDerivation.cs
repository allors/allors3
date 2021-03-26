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

    public class CatalogueDerivation : DomainDerivation
    {
        public CatalogueDerivation(M m) : base(m, new Guid("7B3BA380-A703-4E29-8D2D-0D2F74C6E7D8")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.Catalogue.CatalogueImage),
                new RolePattern(m.Catalogue.LocalisedNames),
                new RolePattern(m.Catalogue.LocalisedDescriptions),
                new RolePattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ this.M.LocalisedText.CatalogueWhereLocalisedName } },
                new RolePattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ this.M.LocalisedText.CatalogueWhereLocalisedDescription } },
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

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (!@this.ExistCatalogueImage)
                {
                    @this.CatalogueImage = @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }
            }
        }
    }
}
