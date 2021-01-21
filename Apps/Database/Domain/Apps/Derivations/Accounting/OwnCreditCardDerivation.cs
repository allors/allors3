
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
    using Database.Derivations;
    using Derivations;

    public class OwnCreditCardDerivation : DomainDerivation
    {
        public OwnCreditCardDerivation(M m) : base(m, new Guid("838dbea6-9123-4cfe-acfe-1c6347ec7ff2")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.OwnCreditCard.GeneralLedgerAccount),
                new ChangedPattern(m.OwnCreditCard.Journal),
                new ChangedPattern(m.OwnCreditCard.CreditCard),
                new ChangedPattern(m.CreditCard.ExpirationYear) { Steps =  new IPropertyType[] {m.CreditCard.OwnCreditCardsWhereCreditCard} },
                new ChangedPattern(m.CreditCard.ExpirationMonth) { Steps =  new IPropertyType[] {m.CreditCard.OwnCreditCardsWhereCreditCard} },
                new ChangedPattern(m.InternalOrganisation.PaymentMethods) { Steps =  new IPropertyType[] {m.InternalOrganisation.PaymentMethods }, OfType = m.OwnCreditCard.Class },
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

                if (@this.ExistCreditCard)
                {
                    if (@this.CreditCard.ExpirationYear <= @this.Session().Now().Year && @this.CreditCard.ExpirationMonth <= @this.Session().Now().Month)
                    {
                        @this.IsActive = false;
                    }
                }

                validation.AssertExistsAtMostOne(@this, @this.M.PaymentMethod.GeneralLedgerAccount, @this.M.PaymentMethod.Journal);
            }
        }
    }
}
