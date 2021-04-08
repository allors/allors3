// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bvba">
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

    public class NonUnifiedPartRule : Rule
    {
        public NonUnifiedPartRule(MetaPopulation m) : base(m, new Guid("280E12F5-C2EA-4D9A-BEDA-D30F229D46A3")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.ProductIdentifications),
                m.NonUnifiedPart.RolePattern(v => v.Keywords),
                m.NonUnifiedPart.RolePattern(v => v.LocalisedNames),
                m.NonUnifiedPart.RolePattern(v => v.SerialisedItems),
                m.NonUnifiedPart.RolePattern(v => v.ProductType),
                m.NonUnifiedPart.RolePattern(v => v.Brand),
                m.NonUnifiedPart.RolePattern(v => v.Model),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName, m.NonUnifiedPart),
                m.Part.AssociationPattern(v => v.PartCategoriesWherePart, m.NonUnifiedPart),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, m.NonUnifiedPart),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                if (!@this.ExistName)
                {
                    @this.Name = "Part " + (@this.PartIdentification() ?? @this.UniqueId.ToString());
                }

                var builder = new StringBuilder();

                builder.Append(@this.Name);

                foreach (LocalisedText localisedText in @this.LocalisedNames)
                {
                    builder.Append(string.Join(", ", localisedText.Text));
                }

                if (@this.ExistProductIdentifications)
                {
                    builder.Append(string.Join(", ", @this.ProductIdentifications.Select(v => v.Identification)));
                }

                if (@this.ExistPartCategoriesWherePart)
                {
                    builder.Append(string.Join(", ", @this.PartCategoriesWherePart.Select(v => v.Name)));
                }

                if (@this.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(", ", @this.SupplierOfferingsWherePart.Select(v => v.Supplier?.PartyName)));
                    builder.Append(string.Join(", ", @this.SupplierOfferingsWherePart.Select(v => v.SupplierProductId)));
                    builder.Append(string.Join(", ", @this.SupplierOfferingsWherePart.Select(v => v.SupplierProductName)));
                }

                if (@this.ExistSerialisedItems)
                {
                    builder.Append(string.Join(", ", @this.SerialisedItems.Select(v => v.SerialNumber)));
                }

                if (@this.ExistProductType)
                {
                    builder.Append(string.Join(", ", @this.ProductType.Name));
                }

                if (@this.ExistBrand)
                {
                    builder.Append(string.Join(", ", @this.Brand.Name));
                }

                if (@this.ExistModel)
                {
                    builder.Append(string.Join(", ", @this.Model.Name));
                }

                builder.Append(string.Join(", ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }
    }
}
