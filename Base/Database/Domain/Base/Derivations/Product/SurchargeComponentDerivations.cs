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
        public class SurchargeComponentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSurchargeComponent = changeSet.Created.Select(v=>v.GetObject()).OfType<SurchargeComponent>();

                foreach(var surchargeComponent in createdSurchargeComponent)
                {
                    validation.AssertAtLeastOne(surchargeComponent, M.SurchargeComponent.Price, M.SurchargeComponent.Percentage);
                    validation.AssertExistsAtMostOne(surchargeComponent, M.SurchargeComponent.Price, M.SurchargeComponent.Percentage);

                    if (surchargeComponent.ExistPrice)
                    {
                        if (!surchargeComponent.ExistCurrency)
                        {
                            surchargeComponent.Currency = surchargeComponent.PricedBy.PreferredCurrency;
                        }

                        validation.AssertExists(surchargeComponent, M.BasePrice.Currency);
                    }

                    BaseOnDeriveVirtualProductPriceComponent(surchargeComponent);
                }

                void BaseOnDeriveVirtualProductPriceComponent(SurchargeComponent surchargeComponent)
                {
                    if (surchargeComponent.ExistProduct)
                    {
                        surchargeComponent.Product.BaseOnDeriveVirtualProductPriceComponent();
                    }
                }
            }

        }

        public static void SurchargeComponentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0c014568-912a-445b-8be7-0b57076dd5aa")] = new SurchargeComponentCreationDerivation();
        }
    }
}
