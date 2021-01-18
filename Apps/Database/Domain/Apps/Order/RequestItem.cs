// <copyright file="RequestItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequestItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.RequestItem, this.M.RequestItem.RequestItemState),
        };

        public bool IsValid => !(this.RequestItemState.IsCancelled || this.RequestItemState.IsRejected);

        internal bool IsDeletable =>
            this.ExistRequestItemState
            && (this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Session).Draft)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Session).Submitted)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Session).Rejected)
                || this.RequestItemState.Equals(new RequestItemStates(this.Strategy.Session).Cancelled));

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedRequest?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.SyncedRequest?.DeniedPermissions.ToArray();
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistRequestItemState)
            {
                this.RequestItemState = new RequestItemStates(this.Strategy.Session).Draft;
            }
        }

        public void AppsCancel(RequestItemCancel method) => this.RequestItemState = new RequestItemStates(this.Strategy.Session).Cancelled;

        public void Sync(Request request) => this.SyncedRequest = request;
    }
}
