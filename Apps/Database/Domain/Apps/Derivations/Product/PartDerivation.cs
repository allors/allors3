// <copyright file="PartDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Meta;

    public class PartDerivation : DomainDerivation
    {
        public PartDerivation(M m) : base(m, new Guid("4F894B49-4922-4FC8-9172-DC600CCDB1CA")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.Part.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Part>())
            {
                @this.SetDisplayName();

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

                if (@this.ExistProductCategoriesWhereAllPart)
                {
                    builder.Append(string.Join(", ", @this.ProductCategoriesWhereAllPart.Select(v => v.Name)));
                }

                if (@this.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(", ", @this.SupplierOfferingsWherePart.Select(v => v.Supplier.PartyName)));
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

                foreach (PartCategory partCategory in @this.PartCategoriesWherePart)
                {
                    builder.Append(string.Join(", ", partCategory.Name));
                }

                builder.Append(string.Join(", ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }
    }
}
