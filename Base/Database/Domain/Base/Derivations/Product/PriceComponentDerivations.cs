// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class BasePriceCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdBasePrices = changeSet.Created.Select(session.Instantiate).OfType<BasePrice>();

                //changeSet.AssociationsByRoleType.TryGetValue(M.BasePrice, out var changedEmployer);
                //var employmentWhereEmployer = changedEmployer?.Select(session.Instantiate).OfType<BasePrice>();

                foreach (var basePrice in createdBasePrices)
                {
                    var internalOrganisations = new Organisations(basePrice.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                    if (!basePrice.ExistPricedBy && internalOrganisations.Count() == 1)
                    {
                        basePrice.PricedBy = internalOrganisations.First();
                    }

                    validation.AssertAtLeastOne(basePrice, M.BasePrice.Part, M.BasePrice.Product, M.BasePrice.ProductFeature);

                    if (basePrice.ExistOrderQuantityBreak)
                    {
                        validation.AddError($"{basePrice} { M.BasePrice.OrderQuantityBreak} {ErrorMessages.BasePriceOrderQuantityBreakNotAllowed}");
                    }

                    if (basePrice.ExistOrderValue)
                    {
                        validation.AddError($"{basePrice} {M.BasePrice.OrderValue} {ErrorMessages.BasePriceOrderValueNotAllowed}");
                    }

                    if (basePrice.ExistPrice)
                    {
                        if (!basePrice.ExistCurrency && basePrice.ExistPricedBy)
                        {
                            basePrice.Currency = basePrice.PricedBy.PreferredCurrency;
                        }

                        validation.AssertExists(basePrice, M.BasePrice.Currency);
                    }

                    if (basePrice.ExistProduct && !basePrice.ExistProductFeature)
                    {
                        // HACK: DerivedRoles
                        basePrice.Product.AddBasePrice(basePrice);
                    }

                    if (basePrice.ExistProductFeature)
                    {
                        basePrice.ProductFeature.AddToBasePrice(basePrice);
                    }

                    if (basePrice.ExistProduct)
                    {
                        basePrice.Product.BaseOnDeriveVirtualProductPriceComponent();
                    }
                }
            }
        }

        public class DiscountComponentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdDiscountComponent = changeSet.Created.Select(session.Instantiate).OfType<DiscountComponent>();

                foreach (var discountComponent in createdDiscountComponent)
                {
                    var internalOrganisations = new Organisations(discountComponent.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                    if (!discountComponent.ExistPricedBy && internalOrganisations.Count() == 1)
                    {
                        discountComponent.PricedBy = internalOrganisations.First();
                    }

                    validation.AssertAtLeastOne(discountComponent, M.DiscountComponent.Price, M.DiscountComponent.Percentage);
                    validation.AssertExistsAtMostOne(discountComponent, M.DiscountComponent.Price, M.DiscountComponent.Percentage);

                    if (discountComponent.ExistPrice)
                    {
                        if (!discountComponent.ExistCurrency && discountComponent.ExistPricedBy)
                        {
                            discountComponent.Currency = discountComponent.PricedBy.PreferredCurrency;
                        }

                        validation.AssertExists(discountComponent, M.BasePrice.Currency);
                    }

                    BaseOnDeriveVirtualProductPriceComponent(discountComponent);
                }

            }

            public void BaseOnDeriveVirtualProductPriceComponent(DiscountComponent discountComponent)
            {
                if (discountComponent.ExistProduct)
                {
                    discountComponent.Product.BaseOnDeriveVirtualProductPriceComponent();
                }
            }
        }

        public static void PriceComponentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d53ec311-55a0-4a5d-8f07-28be47c38303")] = new BasePriceCreationDerivation();
            @this.DomainDerivationById[new Guid("b93f4939-6997-42fa-9370-760fa911cfb9")] = new DiscountComponentCreationDerivation();
        }
    }
}
