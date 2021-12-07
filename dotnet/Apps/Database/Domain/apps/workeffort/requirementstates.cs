// <copyright file="WorkEffortStates.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RequirementStates
    {
        public static readonly Guid CreatedId = new Guid("7435eaa5-4739-4e48-8c6a-3e5645b69d9c");
        public static readonly Guid InProgressId = new Guid("ba14a0d7-da59-4317-b29a-ac8e63bec74c");
        public static readonly Guid CancelledId = new Guid("d1f0e06a-4624-4807-8e5d-184938d48fd6");
        public static readonly Guid ClosedId = new Guid("b2fdc5f7-8475-43ab-a8e5-af5d19f0af7c");

        private UniquelyIdentifiableCache<RequirementState> cache;

        public RequirementState Created => this.Cache[CreatedId];

        public RequirementState InProgress => this.Cache[InProgressId];

        public RequirementState Closed => this.Cache[ClosedId];

        public RequirementState Cancelled => this.Cache[CancelledId];

        private UniquelyIdentifiableCache<RequirementState> Cache => this.cache ??= new UniquelyIdentifiableCache<RequirementState>(this.Transaction);

        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.InventoryTransactionReason);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(CreatedId, v => v.Name = "Created");
            merge(InProgressId, v => v.Name = "In Progress");
            merge(ClosedId, v => v.Name = "Closed");
            merge(CancelledId, v => v.Name = "Cancelled");
        }
    }
}
