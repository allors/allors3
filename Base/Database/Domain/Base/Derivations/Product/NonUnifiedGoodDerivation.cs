// <copyright file="NonUnifiedGoodDerivation.cs" company="Allors bvba">
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
    using Derivations;

    public class NonUnifiedGoodDerivation : DomainDerivation
    {
        public NonUnifiedGoodDerivation(M m) : base(m, new Guid("1D67AC19-4D77-441D-AC98-3F274FADFB2C")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.OrderValue.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var nonUnifiedGood in matches.Cast<NonUnifiedGood>())
            {
                var defaultLocale = nonUnifiedGood.Strategy.Session.GetSingleton().DefaultLocale;
                var settings = nonUnifiedGood.Strategy.Session.GetSingleton().Settings;

                var identifications = nonUnifiedGood.ProductIdentifications;
                identifications.Filter.AddEquals(M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(nonUnifiedGood.Strategy.Session).Good);
                var goodIdentification = identifications.FirstOrDefault();

                if (goodIdentification == null && settings.UseProductNumberCounter)
                {
                    goodIdentification = new ProductNumberBuilder(nonUnifiedGood.Strategy.Session)
                        .WithIdentification(settings.NextProductNumber())
                        .WithProductIdentificationType(new ProductIdentificationTypes(nonUnifiedGood.Strategy.Session).Good).Build();

                    nonUnifiedGood.AddProductIdentification(goodIdentification);
                }

                nonUnifiedGood.ProductNumber = goodIdentification.Identification;

                if (!nonUnifiedGood.ExistProductIdentifications)
                {
                    validation.AssertExists(nonUnifiedGood, M.Good.ProductIdentifications);
                }

                if (!nonUnifiedGood.ExistVariants)
                {
                    validation.AssertExists(nonUnifiedGood, M.NonUnifiedGood.Part);
                }

                if (nonUnifiedGood.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    nonUnifiedGood.Name = nonUnifiedGood.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (nonUnifiedGood.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    nonUnifiedGood.Description = nonUnifiedGood.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                DeriveVirtualProductPriceComponent(nonUnifiedGood);

                var builder = new StringBuilder();
                if (nonUnifiedGood.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", nonUnifiedGood.ProductIdentifications.Select(v => v.Identification)));
                }

                if (nonUnifiedGood.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", nonUnifiedGood.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                builder.Append(string.Join(" ", nonUnifiedGood.Keywords));

                nonUnifiedGood.SearchString = builder.ToString();

                var deletePermission = new Permissions(nonUnifiedGood.Strategy.Session).Get(nonUnifiedGood.Meta.ObjectType, nonUnifiedGood.Meta.Delete, Operations.Execute);
                if (IsDeletable(nonUnifiedGood))
                {
                    nonUnifiedGood.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    nonUnifiedGood.AddDeniedPermission(deletePermission);
                }

            }

            void DeriveVirtualProductPriceComponent(NonUnifiedGood nonUnifiedGood)
            {
                if (!nonUnifiedGood.ExistProductWhereVariant)
                {
                    nonUnifiedGood.RemoveVirtualProductPriceComponents();
                }

                if (nonUnifiedGood.ExistVariants)
                {
                    nonUnifiedGood.RemoveVirtualProductPriceComponents();

                    var priceComponents = nonUnifiedGood.PriceComponentsWhereProduct;

                    foreach (Good product in nonUnifiedGood.Variants)
                    {
                        foreach (PriceComponent priceComponent in priceComponents)
                        {
                            // HACK: DerivedRoles
                            var productDerivedRoles = product;

                            productDerivedRoles.AddVirtualProductPriceComponent(priceComponent);

                            if (priceComponent is BasePrice basePrice && !priceComponent.ExistProductFeature)
                            {
                                productDerivedRoles.AddBasePrice(basePrice);
                            }
                        }
                    }
                }
            }

            bool IsDeletable(NonUnifiedGood nonUnifiedGood) =>
                !nonUnifiedGood.ExistPart &&
                !nonUnifiedGood.ExistDeploymentsWhereProductOffering &&
                !nonUnifiedGood.ExistEngagementItemsWhereProduct &&
                !nonUnifiedGood.ExistGeneralLedgerAccountsWhereCostUnitsAllowed &&
                !nonUnifiedGood.ExistGeneralLedgerAccountsWhereDefaultCostUnit &&
                !nonUnifiedGood.ExistQuoteItemsWhereProduct &&
                !nonUnifiedGood.ExistShipmentItemsWhereGood &&
                !nonUnifiedGood.ExistWorkEffortGoodStandardsWhereUnifiedProduct &&
                !nonUnifiedGood.ExistMarketingPackageWhereProductsUsedIn &&
                !nonUnifiedGood.ExistMarketingPackagesWhereProduct &&
                !nonUnifiedGood.ExistOrganisationGlAccountsWhereProduct &&
                !nonUnifiedGood.ExistProductConfigurationsWhereProductsUsedIn &&
                !nonUnifiedGood.ExistProductConfigurationsWhereProduct &&
                !nonUnifiedGood.ExistRequestItemsWhereProduct &&
                !nonUnifiedGood.ExistSalesInvoiceItemsWhereProduct &&
                !nonUnifiedGood.ExistSalesOrderItemsWhereProduct &&
                !nonUnifiedGood.ExistWorkEffortTypesWhereProductToProduce;
        }
    }
}
