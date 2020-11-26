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

    public class PurchaseOrderCreatedDerivation : DomainDerivation
    {
        public PurchaseOrderCreatedDerivation(M m) : base(m, new Guid("ccff8770-2854-4def-8918-37ab823dbf95")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrder.OrderedBy),
                new ChangedPattern(m.PurchaseOrder.TakenViaSupplier),
                new ChangedPattern(m.PurchaseOrder.AssignedIrpfRegime),
                new ChangedPattern(m.PurchaseOrder.AssignedVatRegime),
                new ChangedPattern(m.PurchaseOrder.AssignedCurrency),
                new ChangedPattern(m.PurchaseOrder.AssignedTakenViaContactMechanism),
                new ChangedPattern(m.PurchaseOrder.AssignedBillToContactMechanism),
                new ChangedPattern(m.PurchaseOrder.AssignedShipToAddress),
                new ChangedPattern(m.PurchaseOrder.Locale),
                new ChangedPattern(m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new ChangedPattern(m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new ChangedPattern(m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new ChangedPattern(m.Organisation.BillingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new ChangedPattern(m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new ChangedPattern(m.Organisation.OrderAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereTakenViaSupplier }},
                new ChangedPattern(m.Party.VatRegime) { Steps = new IPropertyType[] { m.Party.PurchaseOrdersWhereTakenViaSupplier }},
                new ChangedPattern(m.Party.IrpfRegime) { Steps = new IPropertyType[] { m.Party.PurchaseOrdersWhereTakenViaSupplier }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedLocale = @this.Locale ?? @this.OrderedBy?.Locale;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.OrderedBy?.PreferredCurrency;
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.TakenViaSupplier?.VatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? @this.TakenViaSupplier?.IrpfRegime;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.OrderedBy?.ShippingAddress;
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.OrderedBy?.BillingAddress ?? @this.OrderedBy?.GeneralCorrespondence;
                @this.DerivedTakenViaContactMechanism = @this.AssignedTakenViaContactMechanism ?? @this.TakenViaSupplier?.OrderAddress;
            }
        }
    }
}
