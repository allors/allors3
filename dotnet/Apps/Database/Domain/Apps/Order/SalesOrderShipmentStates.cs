// <copyright file="SalesOrderShipmentStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesOrderShipmentStates
    {
        internal static readonly Guid NotApplicableId = new Guid("210672b5-1445-44c4-a6d1-ecd7cf9a6167");
        internal static readonly Guid NotShippedId = new Guid("28256661-0110-42C8-A97E-A4655EFE7974");
        internal static readonly Guid ShippedId = new Guid("CBDBFF96-B5DA-4be3-9B8D-EA785D08C85C");
        internal static readonly Guid PartiallyShippedId = new Guid("40B4EFB9-42A4-43d9-BCE9-39E55FD9D507");
        internal static readonly Guid InProgressId = new Guid("461CCCC8-8661-47C3-868E-CBFE2146063B");

        private UniquelyIdentifiableCache<SalesOrderShipmentState> cache;

        public SalesOrderShipmentState NotApplicable => this.Cache[NotApplicableId];

        public SalesOrderShipmentState NotShipped => this.Cache[NotShippedId];

        public SalesOrderShipmentState Shipped => this.Cache[ShippedId];

        public SalesOrderShipmentState PartiallyShipped => this.Cache[PartiallyShippedId];

        public SalesOrderShipmentState InProgress => this.Cache[InProgressId];

        private UniquelyIdentifiableCache<SalesOrderShipmentState> Cache => this.cache ??= new UniquelyIdentifiableCache<SalesOrderShipmentState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(NotApplicableId, v => v.Name = "N/A");
            merge(NotShippedId, v => v.Name = "Not Shipped");
            merge(PartiallyShippedId, v => v.Name = "Partially Shipped");
            merge(ShippedId, v => v.Name = "Shipped");
            merge(InProgressId, v => v.Name = "In Progress");
        }
    }
}
