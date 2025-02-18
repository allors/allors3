// <copyright file="SalesOrderItemShipmentStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesOrderItemShipmentStates
    {
        internal static readonly Guid NotApplicableId = new Guid("32084bc7-e307-4cb5-b33e-49c255e8932d");
        internal static readonly Guid NotShippedId = new Guid("4DDA233D-EE4A-4716-80C2-C2FD4307C3DC");
        internal static readonly Guid PartiallyShippedId = new Guid("E0FF4A01-CF9B-4dc7-ACF6-145F38F48AD1");
        internal static readonly Guid ShippedId = new Guid("E91BAA87-DF5F-4a6c-B380-B683AD17AE18");
        internal static readonly Guid InProgressId = new Guid("92402D52-2AE7-4DF4-BD2A-BA9E6741F242");

        private UniquelyIdentifiableCache<SalesOrderItemShipmentState> cache;

        public SalesOrderItemShipmentState NotApplicable => this.Cache[NotApplicableId];

        public SalesOrderItemShipmentState NotShipped => this.Cache[NotShippedId];

        public SalesOrderItemShipmentState PartiallyShipped => this.Cache[PartiallyShippedId];

        public SalesOrderItemShipmentState Shipped => this.Cache[ShippedId];

        public SalesOrderItemShipmentState InProgress => this.Cache[InProgressId];

        private UniquelyIdentifiableCache<SalesOrderItemShipmentState> Cache => this.cache ??= new UniquelyIdentifiableCache<SalesOrderItemShipmentState>(this.Transaction);

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
