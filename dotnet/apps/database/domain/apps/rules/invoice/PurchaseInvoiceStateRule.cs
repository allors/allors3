// <copyright file="PurchaseInvoiceStateDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PurchaseInvoiceStateRule : Rule
    {
        public PurchaseInvoiceStateRule(MetaPopulation m) : base(m, new Guid("efdca6b3-b895-4a60-90a2-32f54120126b")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.AmountPaid),
                m.PurchaseInvoice.RolePattern(v => v.GrandTotal),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                var purchaseInvoiceStates = new PurchaseInvoiceStates(@this.Strategy.Transaction);
                var nextState = @this.PurchaseInvoiceState;

                if (@this.ValidInvoiceItems.Any())
                {
                    if (!@this.PurchaseInvoiceState.IsCreated
                        && !@this.PurchaseInvoiceState.IsRevising
                        && !@this.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        if (@this.AmountPaid == 0)
                        {
                            nextState = purchaseInvoiceStates.NotPaid;
                        }
                        else if (@this.AmountPaid >= decimal.Round(@this.GrandTotal, 2))
                        {
                            nextState = purchaseInvoiceStates.Paid;
                        }
                        else
                        {
                            nextState = purchaseInvoiceStates.PartiallyPaid;
                        }
                    }
                }

                if (@this.PurchaseInvoiceState != nextState)
                {
                    @this.PurchaseInvoiceState = nextState;
                }
            }
        }
    }
}
