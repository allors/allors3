// <copyright file="QuoteExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class QuoteExtensions
    {
        public static void AppsOnBuild(this Quote @this, ObjectOnBuild method)
        {
            if (!@this.ExistQuoteState)
            {
                @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Created;
            }
        }

        public static void AppsOnInit(this Quote @this, ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(@this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!@this.ExistIssuer && internalOrganisations.Count() == 1)
            {
                @this.Issuer = internalOrganisations.First();
            }
        }

        public static bool IsDeletable(this Quote @this)
        {
            var productQuote = @this as ProductQuote;

            if(@this is ProductQuote)
            {
                return (@this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Created)
                        || @this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Cancelled)
                        || @this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Rejected))
                       && !@this.ExistRequest
                       && !productQuote.ExistSalesOrderWhereQuote;
            }
            else
            {
                return (@this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Created)
                        || @this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Cancelled)
                        || @this.QuoteState.Equals(new QuoteStates(@this.Strategy.Transaction).Rejected))
                       && !@this.ExistRequest;
            }
        } 

        public static void AppsDelete(this Quote @this, DeletableDelete method)
        {
            var productQuote = @this as ProductQuote;
            var propasal = @this as Proposal;
            var statementOfWork = @this as StatementOfWork;

            if (productQuote != null && productQuote.IsDeletable()
                || propasal != null && propasal.IsDeletable()
                || statementOfWork != null && statementOfWork.IsDeletable())
            {
                foreach (var orderAdjustment in @this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (var item in @this.QuoteItems)
                {
                    item.Delete();
                }
            }
        }

        public static void AppsApprove(this Quote @this, QuoteApprove method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).InProcess;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsSend(this Quote @this, QuoteSend method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).AwaitingAcceptance;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsAccept(this Quote @this, QuoteAccept method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Accepted;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsRevise(this Quote @this, QuoteRevise method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Created;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsReopen(this Quote @this, QuoteReopen method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Created;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsReject(this Quote @this, QuoteReject method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Rejected;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AppsCancel(this Quote @this, QuoteCancel method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Cancelled;
            SetItemState(@this);

            method.StopPropagation = true;
        }

        public static void SetItemState(this Quote @this)
        {
            var quoteItemStates = new QuoteItemStates(@this.Strategy.Transaction);

            foreach (var quoteItem in @this.QuoteItems)
            {
                if (@this.QuoteState.IsCreated)
                {
                    quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Draft;
                }

                if (@this.QuoteState.IsCancelled)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Rejected))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Cancelled;
                    }
                }

                if (@this.QuoteState.IsRejected)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Cancelled))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Rejected;
                    }
                }

                if (@this.QuoteState.IsAwaitingApproval)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.Draft))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).AwaitingApproval;
                    }
                }

                if (@this.QuoteState.IsInProcess)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Cancelled)
                        && !Equals(quoteItem.QuoteItemState, quoteItemStates.Rejected))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).InProcess;
                    }
                }

                if (@this.QuoteState.IsAwaitingAcceptance)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.InProcess))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).AwaitingAcceptance;
                    }
                }

                if (@this.QuoteState.IsAccepted)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.AwaitingAcceptance))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Accepted;
                    }
                }

                if (@this.QuoteState.IsOrdered)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.Accepted))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Ordered;
                    }
                }
            }
        }
    }
}
