// <copyright file="OrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("f3ef0124-e867-4da2-9323-80fbe1f214c2")]
    #endregion
    public partial interface OrderItem : Transitional, Priceable, Deletable, DelegatedAccessControlledObject
    {
        #region Allors
        [Id("7D6B04D2-062C-45B8-96AB-DC41A3DECAF8")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("30493d04-3298-4888-8ee4-b8995d9cd5a1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        BudgetItem BudgetItem { get; set; }

        #region Allors
        [Id("38cd5e9e-7305-4c56-bff7-13918bd9f059")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal PreviousQuantity { get; set; }

        #region Allors
        [Id("454f28cf-bf52-4465-83e4-e871ec36c491")]
        #endregion
        [Workspace(Default)]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal QuantityOrdered { get; set; }

        #region Allors
        [Id("6da42dec-ba03-4615-badb-9113a82ff2f7")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("70f92965-d99a-4a6a-bc27-029eec7b5c2d")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        PurchaseOrder CorrespondingPurchaseOrder { get; set; }

        #region Allors
        [Id("8f06f480-ff7e-4e34-bb7e-6f1271dcc551")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalOrderAdjustment { get; set; }

        #region Allors
        [Id("9674f349-3fcc-495c-b7eb-27b5b580597c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        QuoteItem QuoteItem { get; set; }

        #region Allors
        [Id("9dc87cdb-a35f-4a48-9f99-bf0fe07cad5c")]
        #endregion
        [Workspace(Default)]
        DateTime AssignedDeliveryDate { get; set; }

        #region Allors
        [Id("a1769a74-d832-4ade-be59-a98b17033ca1")]
        #endregion
        [Derived]
        [Workspace(Default)]
        DateTime DeliveryDate { get; set; }

        #region Allors
        [Id("b82c7b21-5ade-40b6-ba5d-62b6384eaaec")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("ce398ebb-3b1e-476e-afd5-d32518542b70")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string ShippingInstruction { get; set; }

        #region Allors
        [Id("dadeac55-1586-47ce-9983-2113179e275d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderItem[] Associations { get; set; }

        #region Allors
        [Id("e31314f8-c9f4-45d0-a1c7-81184091e41b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("9a273bd4-5718-4b8f-a97e-7810f4104a45")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("b09dcce5-f5df-49dd-b62e-9c17860feda2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("a726e7b8-9414-4582-a591-fb98b33bef15")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitIrpf { get; set; }

        #region Allors
        [Id("6b6a1629-a213-4cfa-9fb6-91eec520e865")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("0b3cdfbf-0b75-4a9e-8f5c-40d37908710b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Synced]
        Order SyncedOrder { get; set; }

        #region Allors
        [Id("feeed27a-c421-476c-b233-02d2fb9db76d")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Message { get; set; }

        #region Allors
        [Id("4d816419-3acf-4b1a-9bb7-b14de1ca2e3c")]
        #endregion
        [Required]
        [Workspace(Default)]
        public Guid DerivationTrigger { get; set; }

        #region Allors
        [Id("5368A2C3-9ADF-46A3-9AC0-9C4A03DEAF9A")]
        #endregion
        [Workspace(Default)]
        void Cancel();

        #region Allors
        [Id("29D93AE6-FD73-408F-A8F0-CD05D96CF102")]
        #endregion
        [Workspace(Default)]
        void Reject();

        #region Allors
        [Id("DA334EDA-0CD3-4AB4-89C5-41C69D596C7C")]
        #endregion
        [Workspace(Default)]
        void Approve();

        #region Allors
        [Id("540e9b1b-db95-44c2-961f-0faeef176b2d")]
        #endregion
        [Workspace(Default)]
        void Reopen();
    }
}
