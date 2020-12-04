// <copyright file="Invoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("a6f4eedb-b0b5-491d-bcc0-09d2bc109e86")]
    #endregion
    public partial interface Invoice : Commentable, Printable, Auditable, Transitional, Deletable
    {
        #region Allors
        [Id("8EBB1372-CA22-4639-85FC-D1C14AB0F500")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("1c535b3f-bb97-43a8-bd29-29c4dc267814")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        Currency AssignedCurrency { get; set; }

        #region Allors
        [Id("e4102661-d3dd-4b88-adb4-8d0358fc19c4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        [Indexed]
        Currency DerivedCurrency { get; set; }

        #region Allors
        [Id("2d82521d-30bd-4185-84c7-4dfe08b5ddef")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("4b2eedbb-ec59-4e18-949f-f467e41f6401")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string CustomerReference { get; set; }

        #region Allors
        [Id("4d3f69a0-6e9d-4ba3-acd8-e5dab2a7f401")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal AmountPaid { get; set; }

        #region Allors
        [Id("6b474ddd-c2fd-4db1-bf18-44c86a309d53")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("6ea961d5-89fc-4526-922a-80538ecb5654")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        BillingAccount BillingAccount { get; set; }

        #region Allors
        [Id("7b6ab1ed-845d-4671-bda2-43ad2327ea53")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("7e8de8bd-f1c0-4fa5-a629-34d9d5f71b85")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("7fda150d-44c8-45a9-8048-dfe38d936c3e")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("7a783e3c-9197-4d1a-8291-f95a3b3a799d")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("82541f62-bf0e-4e33-9971-15a5a4fa4469")]
        #endregion
        [Required]
        [Workspace(Default)]
        DateTime InvoiceDate { get; set; }

        #region Allors
        [Id("8798a760-de3d-4210-bd22-165582728f36")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        DateTime EntryDate { get; set; }

        #region Allors
        [Id("94029787-f838-47bb-9617-807a8514a350")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("9eec85a4-e41a-4ca2-82fa-2dc0aa45c9d5")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("5e4bc0b7-8d9a-45ea-a5e7-8c608a286fdf")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("9ff2d65b-0478-41cc-b70b-0df90cdbe190")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("ab342937-1e58-4cd7-99b5-c8a5e7afe317")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string InvoiceNumber { get; set; }

        #region Allors
        [Id("b298c12c-620b-4cf2-b47e-df17afc65552")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Message { get; set; }

        #region Allors
        [Id("c2ecfd15-7662-45b4-99bd-9093ca108d23")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("5e46c4ad-90da-4360-8775-05b0d9dd93b3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("9a12fc10-722b-42c4-aa68-cec89aeb5c12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("84384c93-d213-4abf-bdd3-7d214cea729e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("c7350047-9282-41c8-8d82-4e1f86369e9c")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("2bba9da0-c6d6-4af8-9f93-3bf8c7a46a98")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("fa826458-5423-43dd-b02f-fe2673a2d0f3")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("636a3b83-7157-42a4-bc24-db5419ccb3b7")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }

        #region Allors
        [Id("BBA2D4EA-D31F-4C68-8935-2AC3CC1A267D")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        InvoiceItem[] ValidInvoiceItems { get; set; }

        #region Allors
        [Id("1ef6f9b0-c541-4a8d-9ce2-fb6a330244e9")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableInvoiceNumber { get; set; }

        #region Allors
        [Id("eaa66e01-f597-4c71-86e5-d78652fe926b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Media[] ElectronicDocuments { get; set; }

        #region Allors
        [Id("893734a4-13e6-4740-8c3b-29e28c0137b0")]
        #endregion
        [Required]
        [Workspace(Default)]
        public Guid DerivationTrigger { get; set; }

        #region Allors

        [Id("B9226E72-AD90-4195-9DC7-64A26D12E6A3")]

        #endregion
        [Workspace(Default)]
        void Create();

        #region Allors

        [Id("832244fc-9ac7-4d45-b154-5b49136d97af")]

        #endregion
        [Workspace(Default)]
        void Revise();
    }
}
