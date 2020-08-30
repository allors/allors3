// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PartExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPartExtensions = changeSet.Created.Select(v=>v.GetObject()).OfType<Part>();

                foreach(var partExtensions in createdPartExtensions)
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

        public static void PartExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("caee0321-b3b8-4427-bf10-ba2d5e78351e")] = new PartExtensionsCreationDerivation();
        }
    }
}
