// <copyright file="RequestExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Linq;

    public static partial class RequestExtensions
    {
        public static bool IsDeletable(this Request @this) =>
            // EmailAddress is used whith anonymous request form website
            !@this.ExistEmailAddress
            && (@this.RequestState.Equals(new RequestStates(@this.Strategy.Session).Submitted)
                || @this.RequestState.Equals(new RequestStates(@this.Strategy.Session).Cancelled)
                || @this.RequestState.Equals(new RequestStates(@this.Strategy.Session).Rejected))
            && !@this.ExistQuoteWhereRequest
            && @this.RequestItems.All(v => v.IsDeletable);

        public static void AppsDelete(this Request @this, DeletableDelete method)
        {
            if (@this.IsDeletable())
            {
                foreach (RequestItem item in @this.RequestItems)
                {
                    item.Delete();
                }
            }
        }

        public static void AppsCancel(this Request @this, RequestCancel method) => @this.RequestState = new RequestStates(@this.Strategy.Session).Cancelled;

        public static void AppsReject(this Request @this, RequestReject method) => @this.RequestState = new RequestStates(@this.Strategy.Session).Rejected;

        public static void AppsSubmit(this Request @this, RequestSubmit method) => @this.RequestState = new RequestStates(@this.Strategy.Session).Submitted;

        public static void AppsHold(this Request @this, RequestHold method) => @this.RequestState = new RequestStates(@this.Strategy.Session).PendingCustomer;
    }
}
