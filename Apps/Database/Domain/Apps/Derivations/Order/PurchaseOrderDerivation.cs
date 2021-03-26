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

    public class PurchaseOrderDerivation : DomainDerivation
    {
        public PurchaseOrderDerivation(M m) : base(m, new Guid("C98B629B-12F8-4297-B6DA-FB0C36C56C39")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.TakenViaSupplier),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.TakenViaSubcontractor),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderItems),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderDate),
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.PurchaseOrderItemState)  { Steps = new IPropertyType[] { m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}},
                new RolePattern(m.SupplierRelationship, m.SupplierRelationship.FromDate) { Steps =  new IPropertyType[] {m.SupplierRelationship.Supplier, m.Organisation.PurchaseOrdersWhereTakenViaSupplier } },
                new RolePattern(m.SupplierRelationship, m.SupplierRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.SupplierRelationship.Supplier, m.Organisation.PurchaseOrdersWhereTakenViaSupplier } },
                new RolePattern(m.SubContractorRelationship, m.SubContractorRelationship.FromDate) { Steps =  new IPropertyType[] {m.SubContractorRelationship.SubContractor, m.Organisation.PurchaseOrdersWhereTakenViaSubcontractor } },
                new RolePattern(m.SubContractorRelationship, m.SubContractorRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.SubContractorRelationship.SubContractor, m.Organisation.PurchaseOrdersWhereTakenViaSubcontractor } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistOrderedBy
                    && @this.OrderedBy != @this.CurrentVersion.OrderedBy)
                {
                    validation.AddError($"{@this} {@this.M.PurchaseOrder.OrderedBy} {ErrorMessages.InternalOrganisationChanged}");
                }

                if (!@this.ExistOrderNumber)
                {
                    var year = @this.OrderDate.Year;
                    @this.OrderNumber = @this.OrderedBy?.NextPurchaseOrderNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.OrderedBy?.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.OrderedBy.InvoiceSequence.IsEnforcedSequence ? @this.OrderedBy?.PurchaseOrderNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.PurchaseOrderNumberPrefix;
                    @this.SortableOrderNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.OrderNumber, year.ToString());
                }

                if (@this.TakenViaSupplier is Organisation supplier
                    && !@this.OrderedBy.AppsIsActiveSupplier(@this.TakenViaSupplier, @this.OrderDate))
                {
                    validation.AddError($"{@this} {@this.Meta.TakenViaSupplier} {ErrorMessages.PartyIsNotASupplier}");
                }

                if (@this.TakenViaSubcontractor is Organisation subcontractor
                    && !@this.OrderedBy.AppsIsActiveSubcontractor(@this.TakenViaSubcontractor, @this.OrderDate))
                {
                    validation.AddError($"{@this} {@this.Meta.TakenViaSubcontractor} {ErrorMessages.PartyIsNotASubcontractor}");
                }

                validation.AssertExistsAtMostOne(@this, @this.Meta.TakenViaSupplier, @this.Meta.TakenViaSubcontractor);
                validation.AssertAtLeastOne(@this, @this.Meta.TakenViaSupplier, @this.Meta.TakenViaSubcontractor);

                @this.ValidOrderItems = @this.PurchaseOrderItems.Where(v => v.IsValid).ToArray();
                @this.PreviousTakenViaSupplier = @this.TakenViaSupplier;

                @this.WorkItemDescription = $"PurchaseOrder: {@this.OrderNumber} [{@this.TakenViaSupplier?.PartyName}]";

                @this.ResetPrintDocument();
            }
        }
    }
}
