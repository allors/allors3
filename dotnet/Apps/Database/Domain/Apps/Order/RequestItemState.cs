// <copyright file="RequestItemState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequestItemState
    {
        public bool IsDraft => Equals(this.UniqueId, RequestItemStates.DraftId);

        public bool IsSubmitted => Equals(this.UniqueId, RequestItemStates.SubmittedId);

        public bool IsCancelled => Equals(this.UniqueId, RequestItemStates.CancelledId);

        public bool IsRejected => Equals(this.UniqueId, RequestItemStates.RejectedId);

        public bool IsQuoted => Equals(this.UniqueId, RequestItemStates.QuotedId);
    }
}
