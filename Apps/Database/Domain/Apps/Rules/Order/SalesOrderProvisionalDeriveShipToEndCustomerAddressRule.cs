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

    public class SalesOrderProvisionalDeriveShipToEndCustomerAddressRule : Rule
    {
        public SalesOrderProvisionalDeriveShipToEndCustomerAddressRule(MetaPopulation m) : base(m, new Guid("fbac008b-2068-4d36-b034-748fb7336d17")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipToEndCustomerAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToEndCustomer),
                new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
                new RolePattern(m.Party, m.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress ?? @this.ShipToEndCustomer?.GeneralCorrespondence as PostalAddress;
            }
        }
    }
}
