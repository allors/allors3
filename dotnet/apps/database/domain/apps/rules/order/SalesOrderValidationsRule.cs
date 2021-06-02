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
    using Resources;

    public class SalesOrderValidationsRule : Rule
    {
        public SalesOrderValidationsRule(MetaPopulation m) : base(m, new Guid("271fa746-1edb-4c4b-9e90-acce85235b76")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.SalesOrder.RolePattern(v => v.OrderDate),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.SalesOrder.RolePattern(v => v.DerivedShipToAddress),
                m.SalesOrder.RolePattern(v => v.DerivedBillToContactMechanism),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistTakenBy
                    && @this.TakenBy != @this.CurrentVersion.TakenBy)
                {
                    validation.AddError(@this, this.M.SalesOrder.TakenBy, ErrorMessages.InternalOrganisationChanged);
                }

                if (@this.BillToCustomer?.AppsIsActiveCustomer(@this.TakenBy, @this.OrderDate) == false)
                {
                    validation.AddError(@this, this.M.SalesOrder.BillToCustomer, ErrorMessages.PartyIsNotACustomer);
                }

                if (@this.ShipToCustomer?.AppsIsActiveCustomer(@this.TakenBy, @this.OrderDate) == false)
                {
                    validation.AddError(@this, this.M.SalesOrder.ShipToCustomer, ErrorMessages.PartyIsNotACustomer);
                }

                if (@this.SalesOrderState.IsInProcess)
                {
                    validation.AssertExists(@this, @this.Meta.DerivedShipToAddress);
                    validation.AssertExists(@this, @this.Meta.DerivedBillToContactMechanism);
                }
            }
        }
    }
}
