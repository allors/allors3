// <copyright file="RequestItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class RequestItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.RequestItem, this.M.RequestItem.RequestItemState),
        };

        public bool IsValid => !(this.RequestItemState.IsCancelled || this.RequestItemState.IsRejected);

        internal bool IsDeletable =>
            this.ExistRequestItemState
            && (this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Transaction).Draft)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Transaction).Submitted)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Transaction).Rejected)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Transaction).Cancelled));

        public void AppsDelegateAccess(DelegatedAccessObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedRequest?.SecurityTokens.ToArray();
            }

            if (method.Revocations == null)
            {
                method.Revocations = this.SyncedRequest?.DeniedPermissions.ToArray();
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistRequestItemState)
            {
                this.RequestItemState = new RequestItemStates(this.Strategy.Transaction).Draft;
            }
        }

        public void AppsCancel(RequestItemCancel method)
        {
            this.RequestItemState = new RequestItemStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public void Sync(Request request) => this.SyncedRequest = request;
    }
}
