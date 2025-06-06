// <copyright file="BillingProcesses.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class BillingProcesses
    {
        private static readonly Guid BillingForOrderItemsId = new Guid("AB01CCC2-6480-4FC0-B20E-265AFD41FAE2");
        private static readonly Guid BillingForShipmentItemsId = new Guid("E242D221-7DD6-4BD8-9A4A-E0582EEBECB0");
        private static readonly Guid BillingForWorkEffortsId = new Guid("580A72F2-E428-43C1-8F3E-2A05B7C61712");
        private static readonly Guid BillingForTimeEntriesId = new Guid("0E480590-CA53-40C6-86A8-92871E18FB38");

        private UniquelyIdentifiableCache<BillingProcess> cache;

        public BillingProcess BillingForOrderItems => this.Cache[BillingForOrderItemsId];

        public BillingProcess BillingForShipmentItems => this.Cache[BillingForShipmentItemsId];

        public BillingProcess BillingForWorkEfforts => this.Cache[BillingForWorkEffortsId];

        public BillingProcess BillingForTimeEntries => this.Cache[BillingForTimeEntriesId];

        private UniquelyIdentifiableCache<BillingProcess> Cache => this.cache ??= new UniquelyIdentifiableCache<BillingProcess>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(BillingForOrderItemsId, v =>
            {
                v.Name = "Billing for Order Items";
                v.IsActive = true;
            });

            merge(BillingForShipmentItemsId, v =>
            {
                v.Name = "Billing for Shipment Items";
                v.IsActive = true;
            });

            merge(BillingForWorkEffortsId, v =>
            {
                v.Name = "Billing for Work Efforts";
                v.IsActive = true;
            });

            merge(BillingForTimeEntriesId, v =>
            {
                v.Name = "Billing for Time Entries";
                v.IsActive = true;
            });
        }
    }
}
