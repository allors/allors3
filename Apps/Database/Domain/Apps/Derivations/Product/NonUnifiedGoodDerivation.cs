// <copyright file="NonUnifiedGoodDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;
    using Derivations;

    public class NonUnifiedGoodDerivation : DomainDerivation
    {
        public NonUnifiedGoodDerivation(M m) : base(m, new Guid("1D67AC19-4D77-441D-AC98-3F274FADFB2C")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.NonUnifiedGood.ProductIdentifications),
                new ChangedPattern(m.NonUnifiedGood.LocalisedNames),
                new ChangedPattern(m.NonUnifiedGood.LocalisedDescriptions),
                new ChangedPattern(m.NonUnifiedGood.Keywords),
                new ChangedPattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedName}, OfType = m.NonUnifiedGood.Class },
                new ChangedPattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedDescription}, OfType = m.NonUnifiedGood.Class },
                new ChangedPattern(m.ProductCategory.AllProducts) { Steps = new IPropertyType[]{ m.ProductCategory.AllProducts }, OfType = m.NonUnifiedGood.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
            {
                var defaultLocale = @this.Strategy.Session.GetSingleton().DefaultLocale;

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(@this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Session).Good);
                var goodIdentification = identifications.FirstOrDefault();

                @this.ProductNumber = goodIdentification.Identification;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                var builder = new StringBuilder();
                if (@this.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", @this.ProductIdentifications.Select(v => v.Identification)));
                }

                if (@this.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", @this.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                builder.Append(string.Join(" ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }
    }
}
