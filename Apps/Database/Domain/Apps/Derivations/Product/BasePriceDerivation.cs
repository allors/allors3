// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;
    using Derivations;
    using Resources;

    public class BasePriceDerivation : DomainDerivation
    {
        public BasePriceDerivation(M m) : base(m, new Guid("499B0F1E-F653-4DB6-82D0-190C9738DA5A")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.BasePrice.PricedBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            //changeSet.AssociationsByRoleType.TryGetValue(M.BasePrice, out var changedEmployer);
            //var employmentWhereEmployer = changedEmployer?.Select(session.Instantiate).OfType<BasePrice>();

            foreach (var @this in matches.Cast<BasePrice>())
            {
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistPricedBy && internalOrganisations.Count() == 1)
                {
                    @this.PricedBy = internalOrganisations.First();
                }

                validation.AssertAtLeastOne(@this, this.M.BasePrice.Part, this.M.BasePrice.Product, this.M.BasePrice.ProductFeature);

                if (@this.ExistOrderQuantityBreak)
                {
                    validation.AddError($"{@this} { this.M.BasePrice.OrderQuantityBreak} {ErrorMessages.BasePriceOrderQuantityBreakNotAllowed}");
                }

                if (@this.ExistOrderValue)
                {
                    validation.AddError($"{@this} {this.M.BasePrice.OrderValue} {ErrorMessages.BasePriceOrderValueNotAllowed}");
                }

                if (@this.ExistPrice)
                {
                    if (!@this.ExistCurrency && @this.ExistPricedBy)
                    {
                        @this.Currency = @this.PricedBy.PreferredCurrency;
                    }

                    validation.AssertExists(@this, this.M.BasePrice.Currency);
                }

                if (@this.ExistProduct && !@this.ExistProductFeature)
                {
                    // HACK: DerivedRoles
                    @this.Product.AddBasePrice(@this);
                }

                if (@this.ExistProductFeature)
                {
                    @this.ProductFeature.AddToBasePrice(@this);
                }

                if (@this.ExistProduct)
                {
                    @this.Product.AppsOnDeriveVirtualProductPriceComponent();
                }
            }
        }
    }
}
