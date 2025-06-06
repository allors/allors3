// <copyright file="ProductQuote.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("c2214ff4-d592-4f0d-9215-e431b23dc9c2")]
    #endregion
    [Plural("ProductQuotes")]
    public partial class ProductQuote : Quote, Versioned
    {
        #region inherited properties

        public Media[] ElectronicDocuments { get; set; }

        public string WorkItemDescription { get; set; }

        public QuoteState PreviousQuoteState { get; set; }

        public QuoteState LastQuoteState { get; set; }

        public QuoteState QuoteState { get; set; }

        public InternalOrganisation Issuer { get; set; }

        public string InternalComment { get; set; }

        public QuoteItem[] ValidQuoteItems { get; set; }

        public DateTime ValidFromDate { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public DateTime ValidThroughDate { get; set; }

        public string Description { get; set; }

        public Party Receiver { get; set; }

        public ContactMechanism FullfillContactMechanism { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public VatRate DerivedVatRate { get; set; }

        public VatClause AssignedVatClause { get; set; }

        public VatClause DerivedVatClause { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate DerivedIrpfRate { get; set; }

        public Locale Locale { get; set; }

        public Locale DerivedLocale { get; set; }

        public decimal TotalIrpf { get; set; }

        public decimal TotalExVat { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal TotalSurcharge { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalListPrice { get; set; }

        public OrderAdjustment[] OrderAdjustments { get; set; }

        public Currency AssignedCurrency { get; set; }

        public Currency DerivedCurrency { get; set; }

        public DateTime IssueDate { get; set; }

        public QuoteItem[] QuoteItems { get; set; }

        public string QuoteNumber { get; set; }

        public Request Request { get; set; }

        public Person ContactPerson { get; set; }

        public Revocation[] Revocations { get; set; }

        public Revocation[] TransitionalRevocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public int SortableQuoteNumber { get; set; }

        public Guid DerivationTrigger { get; set; }
        public decimal TotalIrpfInPreferredCurrency { get; set; }

        public decimal TotalExVatInPreferredCurrency { get; set; }

        public decimal TotalVatInPreferredCurrency { get; set; }

        public decimal TotalIncVatInPreferredCurrency { get; set; }

        public decimal GrandTotalInPreferredCurrency { get; set; }

        public decimal TotalSurchargeInPreferredCurrency { get; set; }

        public decimal TotalDiscountInPreferredCurrency { get; set; }

        public decimal TotalShippingAndHandlingInPreferredCurrency { get; set; }

        public decimal TotalFeeInPreferredCurrency { get; set; }

        public decimal TotalExtraChargeInPreferredCurrency { get; set; }

        public decimal TotalBasePriceInPreferredCurrency { get; set; }

        public decimal TotalListPriceInPreferredCurrency { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region Allors
        [Id("b5bf7d21-d81c-48d6-8c17-f7ddf447444b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        public ProductQuoteItemByProduct[] ProductQuoteItemsByProduct { get; set; }

        #region Versioning
        #region Allors
        [Id("A3400F09-19A8-494A-B50A-4081B9E5D174")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public ProductQuoteVersion CurrentVersion { get; set; }

        #region Allors
        [Id("82A8F3CE-9811-4F32-BCE5-D103EFFCBA4B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public ProductQuoteVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Create() { }

        public void SetReadyForProcessing() { }

        public void Reopen() { }

        public void Approve() { }

        public void Send() { }

        public void Accept() { }

        public void Revise() { }

        public void Reject() { }

        public void Cancel() { }

        public void Print() { }

        public void Delete() { }

        public void Copy() { }

        public void Order() { }

        #endregion
    }
}
