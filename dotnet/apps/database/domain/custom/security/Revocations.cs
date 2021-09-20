// <copyright file="Roles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;

    public partial class Revocations
    {
        public static readonly Guid NonUnifiedGoodDeleteRevocationId = new Guid("093a7d72-c9ad-422b-b04f-827305cd7296");
        public static readonly Guid NonUnifiedPartDeleteRevocationId = new Guid("5241e606-2246-435a-aeed-f819d094c3ae");
        public static readonly Guid OrganisationDeleteRevocationId = new Guid("52819a08-c8ac-4e5e-9048-d1c35ff0711c");
        public static readonly Guid PersonDeleteRevocationId = new Guid("f7ce0f2b-14ef-4a4b-8dc9-5f8f9c4fb3c4");
        public static readonly Guid ProductQuoteDeleteRevocationId = new Guid("6e7fabdb-baa1-428b-bc5e-d7935a03e97a");
        public static readonly Guid ProductQuoteSetReadyForProcessingRevocationId = new Guid("30eb2b73-3885-4c11-b329-f49fb3d8c6a4");
        public static readonly Guid ProposalDeleteRevocationId = new Guid("8b267066-0ba9-46e3-bff2-d9ba63373edb");
        public static readonly Guid PurchaseInvoiceCreateSalesInvoiceRevocationId = new Guid("b375129a-7248-4049-8c5a-78165e68d11e");
        public static readonly Guid PurchaseInvoiceDeleteRevocationId = new Guid("43c4f12b-de77-48d5-841f-5bc4249f035f");
        public static readonly Guid PurchaseOrderDeleteRevocationId = new Guid("572139d5-671f-4e33-a7f2-68ade7339295");
        public static readonly Guid PurchaseOrderInvoiceRevocationId = new Guid("caaafa47-b74d-4700-ac23-ee8d4ef44640");
        public static readonly Guid PurchaseOrderQuickReceiveRevocationId = new Guid("eea81a66-3f6f-47d2-a765-3a0a0b3a3633");
        public static readonly Guid PurchaseOrderReviseRevocationId = new Guid("acd5a693-1f4e-4cf7-814e-e8c6bbcdc115");
        public static readonly Guid PurchaseOrderReceivedRevocationId = new Guid("4a5f52f7-ba5f-4193-a75a-23e069cc4dbb");
        public static readonly Guid PurchaseOrderReopenRevocationId = new Guid("5bae5f21-9aad-414b-85de-3383fd6cba29");
        public static readonly Guid PurchaseOrderWriteRevocationId = new Guid("99a3a62e-4367-4f13-851e-810fae4801a6");
        public static readonly Guid PurchaseOrderItemDeleteRevocationId = new Guid("c2b44a75-7d80-4399-aad9-aaa2fa3c9dc3");
        public static readonly Guid RequestForProposalDeleteRevocationId = new Guid("2e09cae8-c4fe-4938-88d1-55aad026a515");

        public Revocation NonUnifiedGoodDeleteRevocation => this.Cache[NonUnifiedGoodDeleteRevocationId];

        public Revocation NonUnifiedPartDeleteRevocation => this.Cache[NonUnifiedPartDeleteRevocationId];

        public Revocation OrganisationDeleteRevocation => this.Cache[OrganisationDeleteRevocationId];

        public Revocation PersonDeleteRevocation => this.Cache[PersonDeleteRevocationId];

        public Revocation ProductQuoteDeleteRevocation => this.Cache[ProductQuoteDeleteRevocationId];

        public Revocation ProductQuoteSetReadyForProcessingRevocation => this.Cache[ProductQuoteSetReadyForProcessingRevocationId];

        public Revocation ProposalDeleteRevocation => this.Cache[ProposalDeleteRevocationId];

        public Revocation PurchaseInvoiceCreateSalesInvoiceRevocation => this.Cache[PurchaseInvoiceCreateSalesInvoiceRevocationId];

        public Revocation PurchaseInvoiceDeleteRevocation => this.Cache[PurchaseInvoiceDeleteRevocationId];

        public Revocation PurchaseOrderDeleteRevocation => this.Cache[PurchaseOrderDeleteRevocationId];

        public Revocation PurchaseOrderInvoiceRevocation => this.Cache[PurchaseOrderInvoiceRevocationId];

        public Revocation PurchaseOrderQuickReceiveRevocation => this.Cache[PurchaseOrderQuickReceiveRevocationId];

        public Revocation PurchaseOrderReviseRevocation => this.Cache[PurchaseOrderReviseRevocationId];

        public Revocation PurchaseOrderReceivedRevocation => this.Cache[PurchaseOrderReceivedRevocationId];

        public Revocation PurchaseOrderReopenRevocation => this.Cache[PurchaseOrderReopenRevocationId];

        public Revocation PurchaseOrderWriteRevocation => this.Cache[PurchaseOrderWriteRevocationId];

        public Revocation RequestForProposalDeleteRevocation => this.Cache[RequestForProposalDeleteRevocationId];

        public Revocation PurchaseOrderItemDeleteRevocation => this.Cache[PurchaseOrderItemDeleteRevocationId];

        protected override void CustomSecure(Security security)
        {
            var merge = this.Cache.Merger().Action();

            merge(NonUnifiedGoodDeleteRevocationId, _ => { });
            merge(NonUnifiedPartDeleteRevocationId, _ => { });
            merge(OrganisationDeleteRevocationId, _ => { });
            merge(PersonDeleteRevocationId, _ => { });
            merge(ProductQuoteDeleteRevocationId, _ => { });
            merge(ProductQuoteSetReadyForProcessingRevocationId, _ => { });
            merge(ProposalDeleteRevocationId, _ => { });
            merge(PurchaseInvoiceCreateSalesInvoiceRevocationId, _ => { });
            merge(PurchaseInvoiceDeleteRevocationId, _ => { });
            merge(PurchaseOrderDeleteRevocationId, _ => { });
            merge(PurchaseOrderInvoiceRevocationId, _ => { });
            merge(PurchaseOrderQuickReceiveRevocationId, _ => { });
            merge(PurchaseOrderReviseRevocationId, _ => { });
            merge(PurchaseOrderReceivedRevocationId, _ => { });
            merge(PurchaseOrderReopenRevocationId, _ => { });
            merge(PurchaseOrderWriteRevocationId, _ => { });
            merge(RequestForProposalDeleteRevocationId, _ => { });
            merge(PurchaseOrderItemDeleteRevocationId, _ => { });
        }
    }
}
