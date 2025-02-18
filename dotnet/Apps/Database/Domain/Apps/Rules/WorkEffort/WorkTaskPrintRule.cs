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

    public class WorkTaskPrintRule : Rule
    {
        public WorkTaskPrintRule(MetaPopulation m) : base(m, new Guid("cd533d3e-922c-4938-a12d-cfacd6c3b9d9")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
            m.WorkEffort.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereAssignment),

            m.WorkTask.RolePattern(v => v.WorkEffortNumber),
            m.WorkTask.RolePattern(v => v.Name),
            m.WorkTask.RolePattern(v => v.Name),
            m.WorkTask.RolePattern(v => v.WorkDone),
            m.WorkTask.RolePattern(v => v.ActualCompletion),
            m.WorkTask.RolePattern(v => v.ScheduledCompletion),
            m.WorkEffortPurpose.RolePattern(v => v.Name, v => v.WorkEffortsWhereWorkEffortPurpose.ObjectType.AsWorkTask),
            m.Facility.RolePattern(v => v.Name, v => v.WorkEffortsWhereFacility.ObjectType.AsWorkTask),
            m.Person.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereContactPerson.ObjectType.AsWorkTask),
            m.TelecommunicationsNumber.RolePattern(v => v.Description, v => v.PartiesWhereCellPhoneNumber.ObjectType.AsPerson.WorkEffortsWhereContactPerson.ObjectType.AsWorkTask),
            m.TelecommunicationsNumber.RolePattern(v => v.Description, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsPerson.WorkEffortsWhereContactPerson.ObjectType.AsWorkTask),
            m.WorkTask.RolePattern(v => v.TotalLabourRevenue),
            m.WorkTask.RolePattern(v => v.TotalOtherRevenue),
            m.WorkTask.RolePattern(v => v.TotalMaterialRevenue),
            m.WorkTask.RolePattern(v => v.TotalSubContractedRevenue),
            m.WorkTask.RolePattern(v => v.GrandTotal),

            m.WorkTask.RolePattern(v => v.Customer),
            m.Party.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.Party.RolePattern(v => v.BillingAddress, v => v.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereBillingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.Party.RolePattern(v => v.ShippingAddress, v => v.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.ObjectType.WorkEffortsWhereCustomer.ObjectType.AsWorkTask),

            m.WorkEffortFixedAssetAssignment.RolePattern(v => v.Assignment, v => v.Assignment.ObjectType.AsWorkTask),
            m.FixedAsset.RolePattern(v => v.DisplayName, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.FixedAsset.RolePattern(v => v.Comment, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.CustomerReferenceNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.ItemNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.SerialNumber, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.ObjectType.SerialisedItems.ObjectType.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.ObjectType.SerialisedItems.ObjectType.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.SerialisedItem.RolePattern(v => v.SerialisedItemCharacteristics, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.ObjectType.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.IUnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.SerialisedItemCharacteristicTypesWhereUnitOfMeasure.ObjectType.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType.ObjectType.SerialisedItemWhereSerialisedItemCharacteristic.ObjectType.WorkEffortFixedAssetAssignmentsWhereFixedAsset.ObjectType.Assignment.ObjectType.AsWorkTask),

            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment.ObjectType.AsWorkTask),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.PurchaseOrderItem, v => v.Assignment.ObjectType.AsWorkTask),
            m.Part.RolePattern(v => v.Name, v => v.PurchaseOrderItemsWherePart.ObjectType.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.PurchaseOrderItem.RolePattern(v => v.Description, v => v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.ObjectType.Assignment.ObjectType.AsWorkTask),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Quantity, v => v.Assignment.ObjectType.AsWorkTask),
            m.UnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.UnifiedProductsWhereUnitOfMeasure.ObjectType.AsPart.PurchaseOrderItemsWherePart.ObjectType.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem.ObjectType.Assignment.ObjectType.AsWorkTask),
            //Gestopt in PurchaseOrderItemAssignmentModel.cs
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
