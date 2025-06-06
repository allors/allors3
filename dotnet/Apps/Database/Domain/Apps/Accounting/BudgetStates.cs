// <copyright file="BudgetStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class BudgetStates
    {
        private static readonly Guid OpenedId = new Guid("D5DE64D5-FE6B-456D-81BE-10BAA8C75C89");
        private static readonly Guid ClosedId = new Guid("4986E755-51D6-4D88-86A4-F22445029D84");
        private static readonly Guid ReopenedId = new Guid("1C435A55-9327-4B32-AE62-07378B11CE0A");

        private UniquelyIdentifiableCache<BudgetState> cache;

        public BudgetState Opened => this.Cache[OpenedId];

        public BudgetState Closed => this.Cache[ClosedId];

        private UniquelyIdentifiableCache<BudgetState> Cache => this.cache ??= new UniquelyIdentifiableCache<BudgetState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(OpenedId, v => v.Name = "Open");
            merge(ClosedId, v => v.Name = "Closed");
            merge(ReopenedId, v => v.Name = "Reopened");
        }
    }
}
