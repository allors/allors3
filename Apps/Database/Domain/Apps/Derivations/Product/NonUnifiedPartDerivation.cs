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

    public class NonUnifiedPartDerivation : DomainDerivation
    {
        public NonUnifiedPartDerivation(M m) : base(m, new Guid("280E12F5-C2EA-4D9A-BEDA-D30F229D46A3")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.NonUnifiedPart.ProductIdentifications),
                new AssociationPattern(m.NonUnifiedPart.Keywords),
                new AssociationPattern(m.NonUnifiedPart.LocalisedNames),
                new AssociationPattern(m.NonUnifiedPart.SerialisedItems),
                new AssociationPattern(m.NonUnifiedPart.ProductType),
                new AssociationPattern(m.NonUnifiedPart.Brand),
                new AssociationPattern(m.NonUnifiedPart.Model),
                new AssociationPattern(m.LocalisedText.Text) { Steps = new IPropertyType[]{ m.LocalisedText.UnifiedProductWhereLocalisedName }, OfType = m.NonUnifiedPart.Class },
                new RolePattern(m.PartCategory.Parts) { OfType = m.NonUnifiedPart.Class  },
                new RolePattern(m.SupplierOffering.Part) { OfType = m.NonUnifiedPart.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                if (!@this.ExistName)
                {
                    @this.Name = "Part " + (@this.PartIdentification() ?? @this.UniqueId.ToString());
                }

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Transaction).Part);
                var partIdentification = identifications.FirstOrDefault();

                @this.ProductNumber = partIdentification?.Identification;

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
