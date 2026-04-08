// <copyright file="WorkEffortPurposes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class WorkEffortPurposes
    {
        private static readonly Guid RefurbishmentId = new Guid("3E8A7F1C-4D02-4B59-8E6A-C1F90D284B73");
        private static readonly Guid MaintenanceId = new Guid("B2D5A8F3-7C14-49E2-A07D-E3F61B8C2940");
        private static readonly Guid RepairId = new Guid("A7C4E2B9-0F83-4D16-B5E8-92A14C67D035");

        private UniquelyIdentifiableCache<WorkEffortPurpose> cache;

        public WorkEffortPurpose Refurbishment => this.Cache[RefurbishmentId];

        public WorkEffortPurpose Maintenance => this.Cache[MaintenanceId];

        public WorkEffortPurpose Repair => this.Cache[RepairId];

        private UniquelyIdentifiableCache<WorkEffortPurpose> Cache => this.cache ??= new UniquelyIdentifiableCache<WorkEffortPurpose>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(RefurbishmentId, v =>
            {
                v.Name = "Refurbishment";
                v.IsActive = true;
            });

            merge(MaintenanceId, v =>
            {
                v.Name = "Maintenance";
                v.IsActive = true;
            });

            merge(RepairId, v =>
            {
                v.Name = "Repair";
                v.IsActive = true;
            });
        }
    }
}
