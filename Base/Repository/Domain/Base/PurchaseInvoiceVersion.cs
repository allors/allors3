// <copyright file="PurchaseInvoiceVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("C23DBDD0-8933-4582-8995-8767EFDA82D5")]
    #endregion
    public partial class PurchaseInvoiceVersion : InvoiceVersion
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string InternalComment { get; set; }

        public Currency Currency { get; set; }

        public string Description { get; set; }

        public OrderAdjustment[] OrderAdjustments { get; set; }

        public string CustomerReference { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal TotalDiscount { get; set; }

        public BillingAccount BillingAccount { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal TotalSurcharge { get; set; }

        public decimal TotalBasePrice { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime EntryDate { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public decimal TotalExVat { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public string InvoiceNumber { get; set; }

        public string Message { get; set; }

        public VatRegime VatRegime { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        #endregion

        #region Allors
        [Id("277D12EB-729E-419A-A9EB-35F30DFFBA15")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Party BilledFrom { get; set; }

        #region Allors
        [Id("57494359-88E5-4032-8E14-D568E191C281")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public ContactMechanism BilledFromContactMechanism { get; set; }

        #region Allors
        [Id("2C2F790B-DC49-4133-BD8F-47917D9E5DC5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Person BilledFromContactPerson { get; set; }

        #region Allors
        [Id("004A1FE5-5EB7-470D-AD91-F62DBAA8E972")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public InternalOrganisation BilledTo { get; set; }

        #region Allors
        [Id("A05AA071-B199-4B8D-A234-5A91435A14E7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Person BilledToContactPerson { get; set; }

        #region Allors
        [Id("2565D7A0-93FF-4023-9CDD-8D85B9589B8E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("33BE845B-F955-4054-BB02-51D2FB49DF76")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public PostalAddress ShipToCustomerAddress { get; set; }

        #region Allors
        [Id("9B9F9513-E27A-4C35-8882-FFAFAD89221F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Person ShipToCustomerContactPerson { get; set; }

        #region Allors
        [Id("499721BD-3B97-47BE-8BF6-0A030D9E1EF4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("BFB058D0-945E-4EAA-B3F4-1C375BB8433B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public ContactMechanism BillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("A10C9C5C-A631-4922-A6CC-183659AACA3C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("A2F6B49E-2DC6-4690-98CC-692076F06AE8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public PaymentMethod BillToCustomerPaymentMethod { get; set; }

        #region Allors
        [Id("C17CCA21-98E3-4919-A10C-2A1DB169C8E9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("C9D07662-ED11-4A82-92C9-B1731BD9DDE9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public PostalAddress ShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("689A34F7-F438-4C20-BAD1-E9F8A36CFA6C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("7751055A-3C59-4723-B7DF-42C377624BE0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Required]
        public PurchaseInvoiceState PurchaseInvoiceState { get; set; }

        #region Allors
        [Id("65E39E93-6445-459A-99E8-0ED388B85B4B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public PurchaseInvoiceItem[] PurchaseInvoiceItems { get; set; }

        #region Allors
        [Id("4A072DD0-0886-4CFF-9DC4-D213854160E7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public PurchaseInvoiceType PurchaseInvoiceType { get; set; }

        #region Allors
        [Id("B3EEBEFD-F12E-4A70-A0DD-A58C89491C03")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public PurchaseOrder[] PurchaseOrders { get; set; }

        #region Allors
        [Id("635FC89C-3AD3-4661-BD24-2EA65002940F")]
        #endregion
        public DateTime DueDate { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
