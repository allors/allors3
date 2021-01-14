// <copyright file="PurchaseOrderVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("5B8C17C9-17BF-4B80-9246-AF7125EAE976")]
    #endregion
    public partial class PurchaseOrderVersion : OrderVersion
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
        [Id("f56f49a3-c122-4bd9-9f66-6e6775700a65")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation OrderedBy { get; set; }

        #region Allors
        [Id("A6D9BF95-9717-4336-A261-4773FBD93CA8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public PurchaseOrderState PurchaseOrderState { get; set; }

        #region Allors
        [Id("C14D2C09-056E-4D36-B0CB-5F5471C28CA1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public PurchaseOrderShipmentState PurchaseOrderShipmentState { get; set; }

        #region Allors
        [Id("EABEB07B-871B-4FCC-A38C-51CBAEBD4F35")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public PurchaseOrderPaymentState PurchaseOrderPaymentState { get; set; }

        #region Allors
        [Id("CAE880EC-B266-4CB2-9FD4-2A0F8B0ACBF8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public PurchaseOrderItem[] PurchaseOrderItems { get; set; }

        #region Allors
        [Id("774CEA12-501D-4C7A-885B-A198079CF74E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Organisation TakenViaSupplier { get; set; }

        #region Allors
        [Id("34274ec5-7e51-431c-bbe5-91a5131fe85c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Organisation TakenViaSubcontractor { get; set; }

        #region Allors
        [Id("8AC728F2-F766-47C4-93B7-15B5D5DC2FF6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public ContactMechanism AssignedTakenViaContactMechanism { get; set; }

        #region Allors
        [Id("b0b71810-c486-4c8d-9f6a-568ff181e0ab")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism DerivedTakenViaContactMechanism { get; set; }

        #region Allors
        [Id("C7B99EF4-7DE8-4214-A598-E6E46608E166")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Person TakenViaContactPerson { get; set; }

        #region Allors
        [Id("A368FB1C-8467-40E9-BC33-47BA5AEA9A0B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public ContactMechanism AssignedBillToContactMechanism { get; set; }

        #region Allors
        [Id("0115fdfc-3192-4b34-aeae-7c5fb392d57a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism DerivedBillToContactMechanism { get; set; }

        #region Allors
        [Id("555F06E3-2C07-4A62-A4D5-E52E64A92362")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("69DDEF12-B6AA-4040-991D-CF1D20A0D5EC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Facility StoredInFacility { get; set; }

        #region Allors
        [Id("57C5DCE6-ACA0-4D03-89B2-4D7CC3AE6E45")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public PostalAddress AssignedShipToAddress { get; set; }

        #region Allors
        [Id("ab0c504f-7706-48fb-a77c-c777f6cef1b3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress DerivedShipToAddress { get; set; }

        #region Allors
        [Id("88691341-493F-4F23-8329-32AC6FC7682E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("df58fb2d-1b2a-4034-822e-7dedc94c465a")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        public PurchaseOrderItemByProduct[] PurchaseOrderItemsByProduct { get; set; }

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
