// <copyright file="RequestExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class RequestExtensions
    {
        public static void AppsOnBuild(this Request @this, ObjectOnBuild method)
        {
            if (!@this.ExistRequestState && !@this.ExistOriginator)
            {
                @this.RequestState = new RequestStates(@this.Transaction()).Anonymous;
            }

            if (!@this.ExistRequestState && @this.ExistOriginator)
            {
                @this.RequestState = new RequestStates(@this.Transaction()).Submitted;
            }
        }

        public static void AppsOnInit(this Request @this, ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(@this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!@this.ExistRecipient && internalOrganisations.Count() == 1)
            {
                @this.Recipient = internalOrganisations.First();
            }
        }

        public static bool IsDeletable(this Request @this) =>
            (@this.RequestState.Equals(new RequestStates(@this.Strategy.Transaction).Submitted)
                || @this.RequestState.Equals(new RequestStates(@this.Strategy.Transaction).Cancelled)
                || @this.RequestState.Equals(new RequestStates(@this.Strategy.Transaction).Rejected))
            && !@this.ExistQuoteWhereRequest
            && @this.RequestItems.All(v => v.IsDeletable);

        public static void AppsDelete(this Request @this, DeletableDelete method)
        {
            if (@this.IsDeletable())
            {
                foreach (var item in @this.RequestItems)
                {
                    item.Delete();
                }
            }
        }

        public static void AppsCancel(this Request @this, RequestCancel method)
        {
            @this.RequestState = new RequestStates(@this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public static void AppsReject(this Request @this, RequestReject method)
        {
            @this.RequestState = new RequestStates(@this.Strategy.Transaction).Rejected;
            method.StopPropagation = true;
        }

        public static void AppsSubmit(this Request @this, RequestSubmit method)
        {
            @this.RequestState = new RequestStates(@this.Strategy.Transaction).Submitted;
            method.StopPropagation = true;
        }

        public static void AppsHold(this Request @this, RequestHold method)
        {
            @this.RequestState = new RequestStates(@this.Strategy.Transaction).PendingCustomer;
            method.StopPropagation = true;
        }
    }
}
