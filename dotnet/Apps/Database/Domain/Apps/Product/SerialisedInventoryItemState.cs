// <copyright file="SerializedInventoryItemState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SerialisedInventoryItemState
    {
        public bool IsGood => this.Equals(new SerialisedInventoryItemStates(this.Strategy.Transaction).Good);

        public bool IsSlightlyDamaged => this.Equals(new SerialisedInventoryItemStates(this.Strategy.Transaction).SlightlyDamaged);

        public bool IsDefective => this.Equals(new SerialisedInventoryItemStates(this.Strategy.Transaction).Defective);

        public bool IsScrap => this.Equals(new SerialisedInventoryItemStates(this.Strategy.Transaction).Scrap);
    }
}
