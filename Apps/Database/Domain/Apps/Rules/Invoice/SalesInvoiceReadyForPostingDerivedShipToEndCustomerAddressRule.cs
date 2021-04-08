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

    public class SalesInvoiceReadyForPostingDerivedShipToEndCustomerAddressRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedShipToEndCustomerAddressRule(MetaPopulation m) : base(m, new Guid("b02041ba-6044-4d89-8405-5e02efa912e5")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.AssignedShipToEndCustomerAddress),
            m.SalesInvoice.RolePattern(v => v.ShipToEndCustomer),
            m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesInvoicesWhereShipToEndCustomer),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress;
            }
        }
    }
}
