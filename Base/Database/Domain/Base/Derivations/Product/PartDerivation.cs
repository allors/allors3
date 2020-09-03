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

    public class PartDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("4F894B49-4922-4FC8-9172-DC600CCDB1CA");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Part.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var partExtensions in matches.Cast<Part>())
            {
                partExtensions.SetDisplayName();

                var builder = new StringBuilder();

                builder.Append(partExtensions.Name);

                foreach (LocalisedText localisedText in partExtensions.LocalisedNames)
                {
                    builder.Append(string.Join(", ", localisedText.Text));
                }

                if (partExtensions.ExistProductIdentifications)
                {
                    builder.Append(string.Join(", ", partExtensions.ProductIdentifications.Select(v => v.Identification)));
                }

                if (partExtensions.ExistProductCategoriesWhereAllPart)
                {
                    builder.Append(string.Join(", ", partExtensions.ProductCategoriesWhereAllPart.Select(v => v.Name)));
                }

                if (partExtensions.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(", ", partExtensions.SupplierOfferingsWherePart.Select(v => v.Supplier.PartyName)));
                    builder.Append(string.Join(", ", partExtensions.SupplierOfferingsWherePart.Select(v => v.SupplierProductId)));
                    builder.Append(string.Join(", ", partExtensions.SupplierOfferingsWherePart.Select(v => v.SupplierProductName)));
                }

                if (partExtensions.ExistSerialisedItems)
                {
                    builder.Append(string.Join(", ", partExtensions.SerialisedItems.Select(v => v.SerialNumber)));
                }

                if (partExtensions.ExistProductType)
                {
                    builder.Append(string.Join(", ", partExtensions.ProductType.Name));
                }

                if (partExtensions.ExistBrand)
                {
                    builder.Append(string.Join(", ", partExtensions.Brand.Name));
                }

                if (partExtensions.ExistModel)
                {
                    builder.Append(string.Join(", ", partExtensions.Model.Name));
                }

                foreach (PartCategory partCategory in partExtensions.PartCategoriesWherePart)
                {
                    builder.Append(string.Join(", ", partCategory.Name));
                }

                builder.Append(string.Join(", ", partExtensions.Keywords));

                partExtensions.SearchString = builder.ToString();
            }
        }
    }
}
