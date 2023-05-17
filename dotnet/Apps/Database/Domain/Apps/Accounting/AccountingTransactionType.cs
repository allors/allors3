// <copyright file="AccountingTransactionType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class AccountingTransactionType
    {
        public bool IsAmortization => this.UniqueId == AccountingTransactionTypes.AmortizationId;
        public bool IsCapitalization => this.UniqueId == AccountingTransactionTypes.CapitalizationId;
        public bool IsCreditLine => this.UniqueId == AccountingTransactionTypes.CreditLineId;
        public bool IsCreditMemo => this.UniqueId == AccountingTransactionTypes.CreditMemoId;
        public bool IsDepreciation => this.UniqueId == AccountingTransactionTypes.DepreciationId;
        public bool IsDisbursement => this.UniqueId == AccountingTransactionTypes.DisbursementId;
        public bool IsExternal => this.UniqueId == AccountingTransactionTypes.ExternalId;
        public bool IsIncomingPayment => this.UniqueId == AccountingTransactionTypes.IncomingPaymentId;
        public bool IsInternal => this.UniqueId == AccountingTransactionTypes.InternalId;
        public bool IsInventory => this.UniqueId == AccountingTransactionTypes.InventoryId;
        public bool IsInventoryReturn => this.UniqueId == AccountingTransactionTypes.InventoryReturnId;
        public bool IsItemVariance => this.UniqueId == AccountingTransactionTypes.ItemVarianceId;
        public bool IsManufacturing => this.UniqueId == AccountingTransactionTypes.ManufacturingId;
        public bool IsNote => this.UniqueId == AccountingTransactionTypes.NoteId;
        public bool IsObligation => this.UniqueId == AccountingTransactionTypes.ObligationId;
        public bool IsOutgoingPayment => this.UniqueId == AccountingTransactionTypes.OutgoingPaymentId;
        public bool IsPayment => this.UniqueId == AccountingTransactionTypes.PaymentId;
        public bool IsPaymentApplication => this.UniqueId == AccountingTransactionTypes.PaymentApplicationId;
        public bool IsPeriodClosing => this.UniqueId == AccountingTransactionTypes.PeriodClosingId;
        public bool IsPurchaseInvoice => this.UniqueId == AccountingTransactionTypes.PurchaseInvoiceId;
        public bool IsReceipt => this.UniqueId == AccountingTransactionTypes.ReceiptId;
        public bool IsSales => this.UniqueId == AccountingTransactionTypes.SalesId;
        public bool IsSalesInvoice => this.UniqueId == AccountingTransactionTypes.SalesInvoiceId;
        public bool IsSalesShipment => this.UniqueId == AccountingTransactionTypes.SalesShipmentId;
        public bool IsShipmentReceipt => this.UniqueId == AccountingTransactionTypes.ShipmentReceiptId;
        public bool IsTaxDue => this.UniqueId == AccountingTransactionTypes.TaxDueId;
    }
}
