// <copyright file="OrderItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("304981F1-CCF3-4946-86E0-5DE1F929BA08")]
    #endregion
    public partial interface OrderItemVersion : PriceableVersion
    {
        #region Allors
        [Id("70F07626-DD20-4BD8-A836-66E1C3DE5EE2")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("6E07F778-88EB-4A3C-8923-4FD508373C3E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        BudgetItem BudgetItem { get; set; }

        #region Allors
        [Id("1B393511-B8B5-488C-B2AB-689C8316EC7D")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal PreviousQuantity { get; set; }

        #region Allors
        [Id("EDD60449-DBC2-4102-BF83-48CE84C528A8")]
        #endregion
        [Workspace(Default)]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal QuantityOrdered { get; set; }

        #region Allors
        [Id("F5147845-F873-4B4A-B3E6-43A7448F6EAB")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("EE2BC870-C91D-4B2A-A66F-7FC1633A88A6")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        PurchaseOrder CorrespondingPurchaseOrder { get; set; }

        #region Allors
        [Id("12184E43-0B6B-4DE9-A36F-CDEAA86A7AFF")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalOrderAdjustment { get; set; }

        #region Allors
        [Id("24AD3FC1-3719-4387-A366-3F11E03F19EC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        QuoteItem QuoteItem { get; set; }

        #region Allors
        [Id("9ED602D2-7520-414E-B699-6A9F3DE8A797")]
        #endregion
        [Workspace(Default)]
        DateTime AssignedDeliveryDate { get; set; }

        #region Allors
        [Id("8303667C-ACD8-4064-9831-9C0129676AB3")]
        #endregion
        [Derived]
        [Workspace(Default)]
        DateTime DeliveryDate { get; set; }

        #region Allors
        [Id("145BFCA4-4B4C-4E35-A0D1-4DAB74481ABE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Indexed]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("CBC89E70-0E91-4CD2-ABCA-C3C2E998339C")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string ShippingInstruction { get; set; }

        #region Allors
        [Id("1C36B7F3-4C5C-4BB8-91C8-4EE7B9468931")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderItem[] Associations { get; set; }

        #region Allors
        [Id("b16be678-3468-447c-b971-34f519820972")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("e62b5afb-21a3-4ab3-b1c8-77a90a605cd3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("e51e9a39-872a-469f-87c7-41e5efc206b3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("d343ff1f-05b5-4d8a-bd62-c89feccb9784")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitIrpf { get; set; }

        #region Allors
        [Id("cd93af05-5f00-415b-9e6a-d53e370b202e")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("E5664F18-C9AA-4590-9882-4DD7FAF3C187")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Message { get; set; }
    }
}
