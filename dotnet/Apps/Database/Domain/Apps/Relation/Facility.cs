// <copyright file="Facility.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Facility
    {
        public bool IsDeletable =>
            !this.ExistFacilitiesWhereParentFacility
            && !this.ExistInventoryItemsWhereFacility
            && !this.ExistInventoryItemTransactionsWhereFacility
            && !this.ExistPartsWhereDefaultFacility
            && !this.ExistPurchaseOrderItemsWhereStoredInFacility
            && !this.ExistPurchaseOrdersWhereStoredInFacility
            && !this.ExistRequirementsWhereFacility
            && !this.ExistSalesInvoiceItemsWhereFacility
            && !this.ExistSalesOrdersWhereOriginFacility
            && !this.ExistSettingsesWhereDefaultFacility
            && !this.ExistShipmentItemsWhereStoredInFacility
            && !this.ExistShipmentReceiptsWhereFacility
            && !this.ExistShipmentRouteSegmentsWhereFromFacility
            && !this.ExistShipmentRouteSegmentsWhereToFacility
            && !this.ExistShipmentsWhereShipFromFacility
            && !this.ExistShipmentsWhereShipToFacility
            && !this.ExistStoresWhereDefaultFacility
            && !this.ExistWorkEffortPartyAssignmentsWhereFacility
            && !this.ExistWorkEffortsWhereFacility;

        public void AppsOnInit(ObjectOnInit method)
        {
            if (!this.ExistParentFacility)
            {
                this.ParentName = string.Empty;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (!this.IsDeletable)
            {
                return;
            }
        }
    }
}
