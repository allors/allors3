// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Derivations;
    using Resources;

    public class BasePriceDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("499B0F1E-F653-4DB6-82D0-190C9738DA5A");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.BasePrice.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            //changeSet.AssociationsByRoleType.TryGetValue(M.BasePrice, out var changedEmployer);
            //var employmentWhereEmployer = changedEmployer?.Select(session.Instantiate).OfType<BasePrice>();

            foreach (var basePrice in matches.Cast<BasePrice>())
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
}
