// <copyright file="SalesOrderVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("FDD29AA9-D2F5-4FA0-8F32-08AD09505577")]
    #endregion
    public partial class SalesOrderVersion : OrderVersion
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

        public string CustomerReference { get; set; }

        public OrderAdjustment[] OrderAdjustments { get; set; }

        public decimal TotalExVat { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalSurcharge { get; set; }

        public OrderItem[] ValidOrderItems { get; set; }

        public string OrderNumber { get; set; }

        public decimal TotalDiscount { get; set; }

        public string Message { get; set; }

        public DateTime EntryDate { get; set; }

        public OrderKind OrderKind { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal GrandTotal { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public decimal TotalIrpf { get; set; }
        #endregion

        #region Allors
        [Id("b39d9e8c-cbf9-4d37-90f3-4f2ac5b00e5d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation TakenBy { get; set; }

        #region Allors
        [Id("DDF41C7B-BF5F-4F60-B39C-5618AB328C42")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public ContactMechanism TakenByContactMechanism { get; set; }

        #region Allors
        [Id("7CA78BDF-9FE3-4494-948B-758CCFBCDE32")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public Person TakenByContactPerson { get; set; }

        #region Allors
        [Id("E665DCB6-6E92-43BA-8E16-EF893F938292")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesOrderState SalesOrderState { get; set; }

        #region Allors
        [Id("CC560B2D-BC23-4CE9-A17D-72C155171FE8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderShipmentState SalesOrderShipmentState { get; set; }

        #region Allors
        [Id("9E3A522A-7F6E-4999-A305-CBBD4CDBB8C4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderInvoiceState SalesOrderInvoiceState { get; set; }

        #region Allors
        [Id("3DA584E0-FAC1-4D24-8E5C-1F631AF2CF52")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderPaymentState SalesOrderPaymentState { get; set; }

        #region Allors
        [Id("943F35C6-F418-4BAD-9F30-ABEF4F19DB48")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipFromAddress { get; set; }

        #region Allors
        [Id("13CBB0CC-126E-4A1F-B873-CF48B6BAA869")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("6A3438D1-2259-4443-A7ED-7B5BE5B7D897")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousShipToCustomer { get; set; }

        #region Allors
        [Id("428F36FF-9B95-4742-8EB9-83107C87B088")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("CDA57AE8-DB34-4FBB-94C3-3BEDD1771077")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("F8D33874-EB71-4FF3-9B13-9CCBEF7EC698")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public Party BillToCustomer { get; set; }

        #region Allors
        [Id("E1AD653D-7C83-4F3D-879C-EF3053DF1288")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousBillToCustomer { get; set; }

        #region Allors
        [Id("A23756EA-C46A-4E6B-B637-4FEDAD3B8FDD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ContactMechanism BillToContactMechanism { get; set; }

        #region Allors
        [Id("218C89A0-0250-42B0-BFA4-97F0909053C6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("F9D9A7E8-CFD7-43E3-9F75-60CBD4DA10E6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("74750CA9-3586-42E3-92CB-E0602D62EE88")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism BillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("1DB7F93E-215F-4786-9E12-F3F36F2FA01A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("1A4D1494-2F21-4FBD-99DF-E8F756CA0274")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("6DE36A70-AC5A-402E-8CB1-AF23BB1247D2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("9BC22E95-660A-46E6-AEB8-5B552A531E98")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("C8FABEAB-4F58-4D30-8CDB-3532E7E88EAF")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party PlacingCustomer { get; set; }

        #region Allors
        [Id("9DCE181F-7EA5-44CD-BB6D-C739153410BC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism PlacingCustomerContactMechanism { get; set; }

        #region Allors
        [Id("E325F42E-3532-4578-A86C-DC36B6634289")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person PlacingCustomerContactPerson { get; set; }

        #region Allors
        [Id("9A852FDF-F111-4FE9-B980-B0DF377B87FB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("896D1356-CEBA-4DB2-8960-92F4DD6DEE62")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal TotalListPrice { get; set; }

        #region Allors
        [Id("E1A92EFA-406F-4A47-906A-BB2FCBD581B4")]
        #endregion
        public bool PartiallyShip { get; set; }

        #region Allors
        [Id("F5BB7592-6BF9-4190-BAE2-C1C092F92EB5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        public Party[] Customers { get; set; }

        #region Allors
        [Id("C101196C-6DA1-4B1C-885E-5B0967CE1DC9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Store Store { get; set; }

        #region Allors
        [Id("AF9225E8-CE1A-4FEE-9D05-476D2810C9CF")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("CCFFB39F-7F02-4C6E-B75C-F348DEB37DC7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public SalesChannel SalesChannel { get; set; }

        #region Allors
        [Id("A4E43616-87BE-4FFE-89FA-B8F7AB2A0C74")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public SalesInvoice ProformaInvoice { get; set; }

        #region Allors
        [Id("BCFB58A6-840A-4AB3-8145-BED773459CD3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public SalesOrderItem[] SalesOrderItems { get; set; }

        #region Allors
        [Id("C440AF2E-CB6A-4614-B0D8-9D09DFD5E568")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ProductQuote Quote { get; set; }

        #region Allors
        [Id("11698673-09EA-480F-B287-3D742FC89A93")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Facility OriginFacility { get; set; }

        #region Allors
        [Id("20518386-6E32-4521-A97E-E5654E06D1E1")]
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
