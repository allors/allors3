// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class SalesOrderProvisionalShipToEndCustomerAddressRule : Rule
    {
        public SalesOrderProvisionalShipToEndCustomerAddressRule(MetaPopulation m) : base(m, new Guid("fbac008b-2068-4d36-b034-748fb7336d17")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedShipToEndCustomerAddress),
                m.SalesOrder.RolePattern(v => v.ShipToEndCustomer),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesOrdersWhereShipToEndCustomer),
                m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesOrdersWhereShipToEndCustomer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
