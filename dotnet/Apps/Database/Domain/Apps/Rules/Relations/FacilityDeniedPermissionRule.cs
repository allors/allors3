// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class FacilityDeniedPermissionRule : Rule
    {
        public FacilityDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("51346b3c-c9f6-4d41-834e-f2d059428b20")) =>
            this.Patterns = new Pattern[]
        {
            m.Facility.AssociationPattern(v => v.FacilitiesWhereParentFacility),
            m.Facility.AssociationPattern(v => v.InventoryItemsWhereFacility),
            m.Facility.AssociationPattern(v => v.InventoryItemTransactionsWhereFacility),
            m.Facility.AssociationPattern(v => v.PartsWhereDefaultFacility),
            m.Facility.AssociationPattern(v => v.PurchaseOrderItemsWhereStoredInFacility),
            m.Facility.AssociationPattern(v => v.PurchaseOrdersWhereStoredInFacility),
            m.Facility.AssociationPattern(v => v.RequirementsWhereFacility),
            m.Facility.AssociationPattern(v => v.SalesInvoiceItemsWhereFacility),
            m.Facility.AssociationPattern(v => v.SalesOrdersWhereOriginFacility),
            m.Facility.AssociationPattern(v => v.SettingsesWhereDefaultFacility),
            m.Facility.AssociationPattern(v => v.ShipmentItemsWhereStoredInFacility),
            m.Facility.AssociationPattern(v => v.ShipmentReceiptsWhereFacility),
            m.Facility.AssociationPattern(v => v.ShipmentRouteSegmentsWhereFromFacility),
            m.Facility.AssociationPattern(v => v.ShipmentRouteSegmentsWhereToFacility),
            m.Facility.AssociationPattern(v => v.ShipmentsWhereShipFromFacility),
            m.Facility.AssociationPattern(v => v.ShipmentsWhereShipToFacility),
            m.Facility.AssociationPattern(v => v.StoresWhereDefaultFacility),
            m.Facility.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereFacility),
            m.Facility.AssociationPattern(v => v.WorkEffortsWhereFacility),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilityDeniedPermission(validation);
            }
        }
    }

    public static class FacilityDeniedPermissionRuleExtensions
    {
        public static void DeriveFacilityDeniedPermission(this Facility @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Transaction()).FacilityDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }
        }
    }
}
