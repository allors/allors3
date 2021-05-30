// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Derivations.Rules;

    public class PaymentApplicationValidationRule : Rule
    {
        public PaymentApplicationValidationRule(MetaPopulation m) : base(m, new Guid("f793cd2b-0b3d-4841-8732-3a36c67e2bac")) =>
            this.Patterns = new Pattern[]
            {
                m.PaymentApplication.RolePattern(v => v.Invoice),
                m.PaymentApplication.RolePattern(v => v.InvoiceItem),
                m.PaymentApplication.RolePattern(v => v.BillingAccount),
                // necessary but unused
                m.PaymentApplication.RolePattern(v => v.AmountApplied),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PaymentApplication>())
            {
                validation.AssertExistsAtMostOne(@this, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);
                validation.AssertAtLeastOne(@this, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);
            }
        }
    }
}
