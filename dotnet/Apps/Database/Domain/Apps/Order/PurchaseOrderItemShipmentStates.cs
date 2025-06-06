// <copyright file="PurchaseOrderItemShipmentStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseOrderItemShipmentStates
    {
        internal static readonly Guid NotReceivedId = new Guid("CF26D4F9-E8AF-4A1D-9841-73B8C5266117");
        internal static readonly Guid PartiallyReceivedId = new Guid("C142144A-8CAE-4D2B-A56B-94BAF236227A");
        internal static readonly Guid ReceivedId = new Guid("AD66619F-BB48-42AF-B019-3E4028AD7B6B");
        internal static readonly Guid ReturnedId = new Guid("0255b70a-13ea-4548-b930-e4251bf294a1");
        internal static readonly Guid NaId = new Guid("6cde2d53-5ab9-49c9-88e5-4484aac27a20");

        private UniquelyIdentifiableCache<PurchaseOrderItemShipmentState> cache;

        public PurchaseOrderItemShipmentState NotReceived => this.Cache[NotReceivedId];

        public PurchaseOrderItemShipmentState PartiallyReceived => this.Cache[PartiallyReceivedId];

        public PurchaseOrderItemShipmentState Received => this.Cache[ReceivedId];

        public PurchaseOrderItemShipmentState Returned => this.Cache[ReturnedId];

        public PurchaseOrderItemShipmentState Na => this.Cache[NaId];

        private UniquelyIdentifiableCache<PurchaseOrderItemShipmentState> Cache => this.cache ??= new UniquelyIdentifiableCache<PurchaseOrderItemShipmentState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(NotReceivedId, v => v.Name = "Not Received");
            merge(PartiallyReceivedId, v => v.Name = "Partially Received");
            merge(ReceivedId, v => v.Name = "Received");
            merge(ReturnedId, v => v.Name = "Returned");
            merge(NaId, v => v.Name = "Shipping Not applicable");
        }
    }
}
