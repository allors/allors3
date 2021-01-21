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
                new ChangedPattern(this.M.NonUnifiedGood.ProductIdentifications),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
            {
                var defaultLocale = @this.Strategy.Session.GetSingleton().DefaultLocale;
                var settings = @this.Strategy.Session.GetSingleton().Settings;

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Session).Good);
                var goodIdentification = identifications.FirstOrDefault();

                if (goodIdentification == null && settings.UseProductNumberCounter)
                {
                    goodIdentification = new ProductNumberBuilder(@this.Strategy.Session)
                        .WithIdentification(settings.NextProductNumber())
                        .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Session).Good).Build();

                    @this.AddProductIdentification(goodIdentification);
                }

                @this.ProductNumber = goodIdentification.Identification;

                if (!@this.ExistProductIdentifications)
                {
                    validation.AssertExists(@this, this.M.Good.ProductIdentifications);
                }

                if (!@this.ExistVariants)
                {
                    validation.AssertExists(@this, this.M.NonUnifiedGood.Part);
                }

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                DeriveVirtualProductPriceComponent(@this);

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

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (IsDeletable(@this))
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
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
                !nonUnifiedGood.ExistGeneralLedgerAccountsWhereAssignedCostUnitsAllowed &&
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
