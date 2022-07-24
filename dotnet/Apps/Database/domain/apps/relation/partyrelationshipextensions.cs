// <copyright file="PartyRelationshipExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class PartyRelationshipExtensions
    {
        public static void AppsOnBuild(this PartyRelationship @this, ObjectOnBuild method)
        {
            if (!@this.ExistFromDate)
            {
                @this.FromDate = @this.Strategy.Transaction.Now();
            }
        }

        public static int? PaymentNetDays(this PartyRelationship @this)
        {
            int? customerPaymentNetDays = null;

            foreach (var agreement in @this.Agreements)
            {
                foreach (var term in agreement.AgreementTerms)
                {
                    if (term.TermType.Equals(new InvoiceTermTypes(@this.Strategy.Transaction).PaymentNetDays))
                    {
                        if (int.TryParse(term.TermValue, out var netDays))
                        {
                            customerPaymentNetDays = netDays;
                        }

                        return customerPaymentNetDays;
                    }
                }
            }

            return null;
        }
    }
}
