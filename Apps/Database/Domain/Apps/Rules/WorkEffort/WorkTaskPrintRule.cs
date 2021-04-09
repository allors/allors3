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

    public class WorkTaskPrintRule : Rule
    {
        public WorkTaskPrintRule(MetaPopulation m) : base(m, new Guid("cd533d3e-922c-4938-a12d-cfacd6c3b9d9")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment, v => v.Assignment),

            m.WorkTask.RolePattern(v => v.WorkEffortNumber),
            m.WorkTask.RolePattern(v => v.Name),
            m.WorkTask.RolePattern(v => v.Name),
            m.WorkTask.RolePattern(v => v.WorkDone),
            m.WorkTask.RolePattern(v => v.ActualCompletion),
            m.WorkTask.RolePattern(v => v.ScheduledCompletion),
            m.WorkEffortPurpose.RolePattern(v => v.Name, v => v.WorkEffortsWhereWorkEffortPurpose.WorkEffort.AsWorkTask),
            m.Facility.RolePattern(v => v.Name, v => v.WorkEffortsWhereFacility.WorkEffort.AsWorkTask),
            m.Person.RolePattern(v => v.PartyName, v => v.WorkEffortsWhereContactPerson.WorkEffort.AsWorkTask),
            m.TelecommunicationsNumber.RolePattern(v => v.Description, v => v.PartiesWhereCellPhoneNumber.Party.AsPerson.WorkEffortsWhereContactPerson.WorkEffort.AsWorkTask),
            m.TelecommunicationsNumber.RolePattern(v => v.Description, v => v.PartiesWhereGeneralPhoneNumber.Party.AsPerson.WorkEffortsWhereContactPerson.WorkEffort.AsWorkTask),
            m.WorkTask.RolePattern(v => v.TotalLabourRevenue),
            m.WorkTask.RolePattern(v => v.TotalMaterialRevenue),
            m.WorkTask.RolePattern(v => v.TotalSubContractedRevenue),
            m.WorkTask.RolePattern(v => v.GrandTotal),

            m.WorkTask.RolePattern(v => v.Customer),
            m.Party.RolePattern(v => v.PartyName, v => v.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.Party.RolePattern(v => v.BillingAddress, v => v.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereBillingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.Party.RolePattern(v => v.ShippingAddress, v => v.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.Party.WorkEffortsWhereCustomer.WorkEffort.AsWorkTask),

            m.WorkEffortFixedAssetAssignment.RolePattern(v => v.Assignment, v => v.Assignment.WorkEffort.AsWorkTask),
            m.FixedAsset.RolePattern(v => v.Name, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.FixedAsset.RolePattern(v => v.Comment, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.CustomerReferenceNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.ItemNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.SerialNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.SerialisedItemsWherePartWhereItem.SerialisedItem.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.SerialisedItemsWherePartWhereItem.SerialisedItem.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.SerialisedItemCharacteristics, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),
            m.IUnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.SerialisedItemCharacteristicTypesWhereUnitOfMeasure.SerialisedItemCharacteristicType.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType.SerialisedItemCharacteristic.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment.WorkEffort.AsWorkTask),

            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment.WorkEffort.AsWorkTask),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.PurchaseOrderItem, v => v.Assignment.WorkEffort.AsWorkTask),
            m.Part.RolePattern(v => v.Name, v => v.PurchaseOrderItemsWherePart.PurchaseOrderItem.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.WorkEffortPurchaseOrderItemAssignment.Assignment.WorkEffort.AsWorkTask),
            m.PurchaseOrderItem.RolePattern(v => v.Description, v => v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.WorkEffortPurchaseOrderItemAssignment.Assignment.WorkEffort.AsWorkTask),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Quantity, v => v.Assignment.WorkEffort.AsWorkTask),
            m.UnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.UnifiedProductsWhereUnitOfMeasure.UnifiedProduct.AsPart.PurchaseOrderItemsWherePart.PurchaseOrderItem.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.WorkEffortPurchaseOrderItemAssignment.Assignment.WorkEffort.AsWorkTask),
            //Gestopt in PurchaseOrderItemAssignmentModel.cs
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
