
// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;
    using Derivations;

    public class OwnCreditCardInternalOrganisationPaymentMethodsRule : Rule
    {
        public OwnCreditCardInternalOrganisationPaymentMethodsRule(MetaPopulation m) : base(m, new Guid("a85e1656-b65b-4af9-83ef-fb818c7527c6")) =>
            this.Patterns = new Pattern[]
            {
                m.OwnCreditCard.RolePattern(v => v.GeneralLedgerAccount),
                m.OwnCreditCard.RolePattern(v => v.Journal),
                m.PaymentMethod.AssociationPattern(v => v.InternalOrganisationWherePaymentMethod, m.OwnCreditCard),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OwnCreditCard>())
            {
                if (@this.ExistInternalOrganisationWherePaymentMethod && @this.InternalOrganisationWherePaymentMethod.DoAccounting)
                {
                    validation.AssertAtLeastOne(@this, @this.M.PaymentMethod.GeneralLedgerAccount, @this.M.PaymentMethod.Journal);
                }

                validation.AssertExistsAtMostOne(@this, @this.M.PaymentMethod.GeneralLedgerAccount, @this.M.PaymentMethod.Journal);

            }
        }
    }
}
