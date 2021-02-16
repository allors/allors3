// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class GoodDerivation : DomainDerivation
    {
        public GoodDerivation(M m) : base(m, new Guid("1e9d2e93-5f3d-4682-9051-a0fd3f89d68e")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Good.ProductIdentifications),
                new ChangedPattern(m.Good.LocalisedNames),
                new ChangedPattern(m.Good.LocalisedDescriptions),
                new ChangedPattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedName }, OfType = m.Good.Interface },
                new ChangedPattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedDescription}, OfType = m.Good.Interface  },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Good>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Transaction).Good);
                var goodIdentification = identifications.FirstOrDefault();

                @this.ProductNumber = goodIdentification?.Identification;

                if (!@this.ExistProductIdentifications)
                {
                    cycle.Validation.AssertExists(@this, this.M.Good.ProductIdentifications);
                }

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }
            }
        }
    }
}
