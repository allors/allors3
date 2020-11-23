// <copyright file="SalesInvoiceVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("0982A8D9-4F69-4F4A-A7C2-2AC7ABBE9924")]
    #endregion
    public partial class SalesInvoiceVersion : InvoiceVersion
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

        public Currency AssignedCurrency { get; set; }

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

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        #endregion

        #region Allors
        [Id("1D302901-A2D1-4D13-945A-3F61A96E474B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation BilledFrom { get; set; }

        #region Allors
        [Id("55772E8F-4911-4746-83F8-654F10859D6B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism AssignedBilledFromContactMechanism { get; set; }

        #region Allors
        [Id("b365caa0-82af-43c1-b9c2-4cab0e02c70e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism DerivedBilledFromContactMechanism { get; set; }

        #region Allors
        [Id("F1B241E8-26F6-4203-B1BF-E7F03942D76F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BilledFromContactPerson { get; set; }

        #region Allors
        [Id("92C03EDC-A5B4-4AFD-BCAE-533FC0D77DA4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party BillToCustomer { get; set; }

        #region Allors
        [Id("C90E49CC-63DC-41A6-B5C9-24B38EFDD8E3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousBillToCustomer { get; set; }

        #region Allors
        [Id("5A126FD7-5CFD-4633-8F8B-E41879A9AEA2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ContactMechanism AssignedBillToContactMechanism { get; set; }

        #region Allors
        [Id("696b52bf-0b72-4796-ab7e-1bb59c26a4f1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public ContactMechanism DerivedBillToContactMechanism { get; set; }

        #region Allors
        [Id("666B66B6-3271-47B4-A92E-DB26DE179F61")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("3901A676-CC14-4BA4-AA83-944C9B23DE11")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("C98647B8-E3DC-4EF9-9109-E7239D6AF534")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Party PreviousShipToCustomer { get; set; }

        #region Allors
        [Id("417B7D53-AA3B-4881-A491-1FC539D2C013")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipToAddress { get; set; }

        #region Allors
        [Id("c5e72ba5-ba0e-4558-88b1-3b3689f6af69")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress DerivedShipToAddress { get; set; }

        #region Allors
        [Id("D4DD296B-E609-4345-B51C-6FC3D15E8CD9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("A90BD0E6-E6B4-4B39-BAF3-D890E5500582")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("A888A8F0-8E16-4318-BB09-9C195D716108")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism AssignedBillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("864d5150-10ae-48cb-966c-7c3ae4dc37bc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism DerivedBillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("17B0CD8A-8F59-4C7A-B464-7ED1403788C9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("5CA9F52A-2C35-4BBF-8682-7D39E122FDD3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("F24B4583-0031-4A87-8395-B0BB6122BE99")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("85090aa2-13d3-4836-a647-e8e07fa5f30c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress DerivedShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("1F2605FB-7D41-4691-8BE5-DAD06265702D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("CEAFD664-2CA9-41B7-BC13-12B66AE8BD7C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesInvoiceState SalesInvoiceState { get; set; }

        #region Allors
        [Id("A1521081-CF30-4558-A73E-2E71287E9826")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal TotalListPrice { get; set; }

        #region Allors
        [Id("C4538A99-07A4-49CF-8DA5-A608626B1381")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesInvoiceType SalesInvoiceType { get; set; }

        #region Allors
        [Id("5DF1979F-4121-432C-92E4-CD7794105F4C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod AssignedPaymentMethod { get; set; }

        #region Allors
        [Id("8f0b6875-e860-419d-bb9c-3a226a171528")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod DerivedPaymentMethod { get; set; }

        #region Allors
        [Id("3E595F3B-D845-4141-A4F8-E055B01AFDBE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public SalesInvoiceItem[] SalesInvoiceItems { get; set; }

        #region Allors
        [Id("33E0CEBA-6951-40F3-A5EA-BE0DD5217564")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesChannel SalesChannel { get; set; }

        #region Allors
        [Id("A407D8AC-1951-4DCC-9224-399139D6AFEB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public Party[] Customers { get; set; }

        #region Allors
        [Id("B87EFDD6-DE6B-4432-AD22-5E3FBB0218FE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Store Store { get; set; }

        #region Allors
        [Id("4D8D082D-2A8A-4D9B-9580-57E9A8166A57")]
        #endregion
        [Workspace(Default)]
        public int PaymentDays { get; set; }

        #region Allors
        [Id("F5DB1C05-8EDA-40AA-89EC-388394F8EEEA")]
        #endregion
        public DateTime DueDate { get; set; }

        #region Allors
        [Id("906EB925-D04D-44F6-9E1C-13438200C1F4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public VatClause DerivedVatClause { get; set; }

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
