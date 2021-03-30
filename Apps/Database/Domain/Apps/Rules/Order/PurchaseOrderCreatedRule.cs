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
    using Database.Derivations;
    using Resources;

    public class PurchaseOrderCreatedRule : Rule
    {
        public PurchaseOrderCreatedRule(M m) : base(m, new Guid("ccff8770-2854-4def-8918-37ab823dbf95")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderState),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.TakenViaSupplier),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedIrpfRegime),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedVatRegime),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedCurrency),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedTakenViaContactMechanism),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedBillToContactMechanism),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedShipToAddress),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.Locale),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderDate),
                new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.BillingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.OrderAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereTakenViaSupplier }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedLocale = @this.Locale ?? @this.OrderedBy?.Locale;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.OrderedBy?.PreferredCurrency;
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.OrderedBy?.ShippingAddress;
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.OrderedBy?.BillingAddress ?? @this.OrderedBy?.GeneralCorrespondence;
                @this.DerivedTakenViaContactMechanism = @this.AssignedTakenViaContactMechanism ?? @this.TakenViaSupplier?.OrderAddress;

                if (@this.ExistOrderDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                }
            }
        }
    }
}
