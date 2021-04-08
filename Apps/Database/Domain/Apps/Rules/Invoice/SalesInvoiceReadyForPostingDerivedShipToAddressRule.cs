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
    using Resources;

    public class SalesInvoiceReadyForPostingDerivedShipToAddressRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedShipToAddressRule(MetaPopulation m) : base(m, new Guid("1c3e0a2b-f8df-4702-95bc-4f6041f87de3")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.AssignedShipToAddress),
            m.SalesInvoice.RolePattern(v => v.ShipToCustomer),
            m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesInvoicesWhereShipToCustomer),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;
            }
        }
    }
}
