// <copyright file="PickListState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PickListState
    {
        public bool IsCreated => Equals(this.UniqueId, PickListStates.CreatedId);

        public bool IsPicked => Equals(this.UniqueId, PickListStates.PickedId);

        public bool IsCancelled => Equals(this.UniqueId, PickListStates.CancelledId);

        public bool IsOnHold => Equals(this.UniqueId, PickListStates.OnHoldId);
    }
}
