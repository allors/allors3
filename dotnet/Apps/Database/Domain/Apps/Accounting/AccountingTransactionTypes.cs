// <copyright file="AccountingTransactionTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class AccountingTransactionTypes
    {
        public static readonly Guid AmortizationId = new Guid("70afee4b-1b66-4bf1-af2d-9830c350ef5c");
        public static readonly Guid CapitalizationId = new Guid("abfe1bcb-00a2-43b9-877e-29f4fb612ae3");
        public static readonly Guid CreditLineId = new Guid("d2d99a7f-87a4-4b5c-b9e9-a75d985482f2");
        public static readonly Guid CreditMemoId = new Guid("5f0e7727-ad57-4335-9b08-fa8d6a4b04a0");
        public static readonly Guid DepreciationId = new Guid("684093d8-2681-4f43-af63-048f3f2e89b3");
        public static readonly Guid DisbursementId = new Guid("1ae51a55-6803-4c14-95ac-0d27be006c13");
        public static readonly Guid ExternalId = new Guid("663e5ae5-2bf9-43ca-a2fb-36596784734e");
        public static readonly Guid IncomingPaymentId = new Guid("6a1a350d-2800-44ef-89e8-7296f2a73ad5");
        public static readonly Guid InternalId = new Guid("817ec6c6-7545-4df2-ba5d-e084da7ae644");
        public static readonly Guid InventoryId = new Guid("6f482c8c-61ea-4006-9bbb-a43a6e1c627c");
        public static readonly Guid InventoryReturnId = new Guid("82c6828b-0674-401f-9ba7-82b4b0e8edb2");
        public static readonly Guid ItemVarianceId = new Guid("1e800752-0307-46b5-af2b-eba09a8185e1");
        public static readonly Guid ManufacturingId = new Guid("a34494f3-23a7-4cdf-9883-679e3ab4649e");
        public static readonly Guid NoteId = new Guid("ad9b23db-a521-4b82-8d7a-8bb889c810ef");
        public static readonly Guid ObligationId = new Guid("4a803164-ad6e-401e-918b-4974bedc74ee");
        public static readonly Guid OutgoingPaymentId = new Guid("832016e8-b71a-4fd7-84b0-2a606be4c76c");
        public static readonly Guid PaymentId = new Guid("f4c518d4-b3e2-4481-ae17-58679ee9c645");
        public static readonly Guid PaymentApplicationId = new Guid("b1eb2745-e6d9-45cd-8766-0bb935277a8c");
        public static readonly Guid PeriodClosingId = new Guid("ef61af53-445b-4c75-888b-73e3e062b48e");
        public static readonly Guid PurchaseInvoiceId = new Guid("d4492bdb-2293-4b1e-8453-1da954a9d2d5");
        public static readonly Guid ReceiptId = new Guid("7c84f02b-8163-4045-b48c-8e02bf9dd1bd");
        public static readonly Guid SalesId = new Guid("5cee2f17-2cc7-4130-a2ea-9cfcd2f82f27");
        public static readonly Guid SalesInvoiceId = new Guid("22e61348-7be6-4b2d-a4f7-8a56dd4c02ed");
        public static readonly Guid SalesShipmentId = new Guid("50be7523-e01f-4f04-abe9-c12639ced372");
        public static readonly Guid ShipmentReceiptId = new Guid("eb12db64-5d06-4d06-959b-32daf62661dd");
        public static readonly Guid TaxDueId = new Guid("9b2bda24-d181-4505-b36c-430b58eb60fb");

        private UniquelyIdentifiableCache<AccountingTransactionType> cache;

        public AccountingTransactionType Amortization => this.Cache[AmortizationId];

        public AccountingTransactionType Capitalization => this.Cache[CapitalizationId];

        public AccountingTransactionType CreditLine => this.Cache[CreditLineId];

        public AccountingTransactionType CreditMemo => this.Cache[CreditMemoId];

        public AccountingTransactionType Depreciation => this.Cache[DepreciationId];

        public AccountingTransactionType Disbursement => this.Cache[DisbursementId];

        public AccountingTransactionType External => this.Cache[ExternalId];

        public AccountingTransactionType IncomingPayment => this.Cache[IncomingPaymentId];

        public AccountingTransactionType Internal => this.Cache[InternalId];

        public AccountingTransactionType Inventory => this.Cache[InventoryId];

        public AccountingTransactionType InventoryReturn => this.Cache[InventoryReturnId];

        public AccountingTransactionType ItemVariance => this.Cache[ItemVarianceId];

        public AccountingTransactionType Manufacturing => this.Cache[ManufacturingId];

        public AccountingTransactionType Note => this.Cache[NoteId];

        public AccountingTransactionType Obligation => this.Cache[ObligationId];

        public AccountingTransactionType OutgoingPayment => this.Cache[OutgoingPaymentId];

        public AccountingTransactionType Payment => this.Cache[PaymentId];

        public AccountingTransactionType PaymentApplication => this.Cache[PaymentApplicationId];

        public AccountingTransactionType PeriodClosing => this.Cache[PeriodClosingId];

        public AccountingTransactionType PurchaseInvoice => this.Cache[PurchaseInvoiceId];

        public AccountingTransactionType Receipt => this.Cache[ReceiptId];

        public AccountingTransactionType Sales => this.Cache[SalesId];

        public AccountingTransactionType SalesInvoice => this.Cache[SalesInvoiceId];

        public AccountingTransactionType SalesShipment => this.Cache[SalesShipmentId];

        public AccountingTransactionType ShipmentReceipt => this.Cache[ShipmentReceiptId];

        public AccountingTransactionType TaxDue => this.Cache[TaxDueId];

        private UniquelyIdentifiableCache<AccountingTransactionType> Cache => this.cache ??= new UniquelyIdentifiableCache<AccountingTransactionType>(this.Transaction);

        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Locale);

        protected override void AppsSetup(Setup setup)
        {
            this.Upgrade();
        }

        public void Upgrade()
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(AmortizationId, v =>
            {
                v.Name = "Amortization";
                localisedName.Set(v, dutchLocale, "Amortisatie");
                v.IsActive = true;
            });

            merge(CapitalizationId, v =>
            {
                v.Name = "Capitalization";
                localisedName.Set(v, dutchLocale, "Kapitalisatie");
                v.IsActive = true;
            });

            merge(CreditLineId, v =>
            {
                v.Name = "Credit Line";
                localisedName.Set(v, dutchLocale, "Krediet nota");
                v.IsActive = true;
            });

            merge(CreditMemoId, v =>
            {
                v.Name = "Credit Memo";
                localisedName.Set(v, dutchLocale, "Krediet nota");
                v.IsActive = true;
            });

            merge(DepreciationId, v =>
            {
                v.Name = "Depreciation";
                localisedName.Set(v, dutchLocale, "Afschrijving");
                v.IsActive = true;
            });

            merge(DisbursementId, v =>
            {
                v.Name = "Disbursement";
                localisedName.Set(v, dutchLocale, "Uitbetaling");
                v.IsActive = true;
            });

            merge(ExternalId, v =>
            {
                v.Name = "External";
                localisedName.Set(v, dutchLocale, "Extern");
                v.IsActive = true;
            });

            merge(IncomingPaymentId, v =>
            {
                v.Name = "Incoming Payment";
                localisedName.Set(v, dutchLocale, "Inkomende betaling");
                v.IsActive = true;
            });

            merge(InternalId, v =>
            {
                v.Name = "Internal";
                localisedName.Set(v, dutchLocale, "Intern");
                v.IsActive = true;
            });

            merge(InventoryId, v =>
            {
                v.Name = "Inventory";
                localisedName.Set(v, dutchLocale, "Voorraad");
                v.IsActive = true;
            });

            merge(InventoryReturnId, v =>
            {
                v.Name = "Inventory from Return";
                localisedName.Set(v, dutchLocale, "Voorraad van retour");
                v.IsActive = true;
            });

            merge(ItemVarianceId, v =>
            {
                v.Name = "Inventory Item Variance";
                localisedName.Set(v, dutchLocale, "Vooraad afwijking");
                v.IsActive = true;
            });

            merge(ManufacturingId, v =>
            {
                v.Name = "Manufacturing";
                localisedName.Set(v, dutchLocale, "Productie");
                v.IsActive = true;
            });

            merge(NoteId, v =>
            {
                v.Name = "Note";
                localisedName.Set(v, dutchLocale, "Notitie");
                v.IsActive = true;
            });

            merge(ObligationId, v =>
            {
                v.Name = "Obligation";
                localisedName.Set(v, dutchLocale, "Verplichting");
                v.IsActive = true;
            });

            merge(OutgoingPaymentId, v =>
            {
                v.Name = "Outgoing Payment";
                localisedName.Set(v, dutchLocale, "Uitgaande betaling");
                v.IsActive = true;
            });

            merge(PaymentId, v =>
            {
                v.Name = "Payment";
                localisedName.Set(v, dutchLocale, "Betaling");
                v.IsActive = true;
            });

            merge(PaymentApplicationId, v =>
            {
                v.Name = "Payment Application";
                localisedName.Set(v, dutchLocale, "Betaling toegepast");
                v.IsActive = true;
            });

            merge(PeriodClosingId, v =>
            {
                v.Name = "Period Closing";
                localisedName.Set(v, dutchLocale, "Periode afsluiting");
                v.IsActive = true;
            });

            merge(PurchaseInvoiceId, v =>
            {
                v.Name = "Purchase Invoice";
                localisedName.Set(v, dutchLocale, "Inkoopfactuur");
                v.IsActive = true;
            });

            merge(ReceiptId, v =>
            {
                v.Name = "Receipt";
                localisedName.Set(v, dutchLocale, "Ontvangst");
                v.IsActive = true;
            });

            merge(SalesId, v =>
            {
                v.Name = "Sales";
                localisedName.Set(v, dutchLocale, "Verkopen");
                v.IsActive = true;
            });

            merge(SalesInvoiceId, v =>
            {
                v.Name = "Sales Invoice";
                localisedName.Set(v, dutchLocale, "Verkoopfactuur");
                v.IsActive = true;
            });

            merge(SalesShipmentId, v =>
            {
                v.Name = "Sales Shipment";
                localisedName.Set(v, dutchLocale, "Verkoop verzending");
                v.IsActive = true;
            });

            merge(ShipmentReceiptId, v =>
            {
                v.Name = "Shipment Receipt";
                localisedName.Set(v, dutchLocale, "Verzending bon");
                v.IsActive = true;
            });

            merge(TaxDueId, v =>
            {
                v.Name = "Tax Due";
                localisedName.Set(v, dutchLocale, "Verschuldigde belasting");
                v.IsActive = true;
            });
        }
    }
}
