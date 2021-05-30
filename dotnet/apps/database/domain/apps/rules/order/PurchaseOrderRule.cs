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

    public class PurchaseOrderRule : Rule
    {
        public PurchaseOrderRule(MetaPopulation m) : base(m, new Guid("C98B629B-12F8-4297-B6DA-FB0C36C56C39")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSubcontractor),
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderItems),
                m.PurchaseOrder.RolePattern(v => v.OrderDate),
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.SupplierRelationship.RolePattern(v => v.FromDate, v => v.Supplier.Organisation.PurchaseOrdersWhereTakenViaSupplier),
                m.SupplierRelationship.RolePattern(v => v.ThroughDate, v => v.Supplier.Organisation.PurchaseOrdersWhereTakenViaSupplier),
                m.SubContractorRelationship.RolePattern(v => v.FromDate, v => v.SubContractor.Organisation.PurchaseOrdersWhereTakenViaSubcontractor),
                m.SubContractorRelationship.RolePattern(v => v.ThroughDate, v => v.SubContractor.Organisation.PurchaseOrdersWhereTakenViaSubcontractor),
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
