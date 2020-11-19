// <copyright file="Order.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7dde949a-6f54-4ece-92b3-d269f50ef9d9")]
    #endregion
    public partial interface Order : Printable, Commentable, Localised, Auditable, Transitional, Deletable
    {
        #region Allors
        [Id("962215D2-4461-4BD3-9A98-F1A085B2343F")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("2DD5B3C2-1C24-4AFA-A5E0-930BB943E93E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        Currency Currency { get; set; }

        #region Allors
        [Id("45b3b293-b746-4d6d-9da7-e2378694f734")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string CustomerReference { get; set; }

        #region Allors
        [Id("6509263c-a11e-4554-b13d-4fa075fa8ed9")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("7374e62f-0f0b-49de-8c70-9ef224a706b1")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("7d23c0ec-57c9-4129-b7d1-ea4ec1ab83dd")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("ba6e8dd3-ad74-4ead-96df-d9ba2e067bfc")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("751cb60a-b8ba-473a-ab95-0909bd2bc61c")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("8592f390-a9fb-4275-93c2-b7e73afa2307")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("d0730f9e-3217-45b3-a5f8-6ae3a5174050")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("faa16c88-2ca0-4eea-847e-793ab84d7dea")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("c0443691-9d8e-4192-a51e-09abb1dbbf24")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }

        #region Allors
        [Id("f636599a-9c61-4952-abcf-963e6f6bdcd8")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("41c58b71-f1f7-49c1-b852-8281bb8c8969")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("af6aaba3-20df-48b0-95ea-08aaad9b1183")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("73521788-7e0e-4ea2-9961-1a58f68cde5c")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("7c04f907-4254-4b59-861a-7b545c12b3d3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        OrderItem[] ValidOrderItems { get; set; }

        #region Allors
        [Id("7db0e5f7-8a23-4be8-beba-8ddfd1972856")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Size(256)]
        string OrderNumber { get; set; }

        #region Allors
        [Id("8c972fae-b3ba-4e88-b769-d59c14325b00")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Message { get; set; }

        #region Allors
        [Id("7E66C6E0-F4BD-4085-AD5E-7012B576AFC2")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("a5875c41-9f08-49d0-9961-19a656c7e0cc")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        DateTime EntryDate { get; set; }

        #region Allors
        [Id("b205a525-fc61-436d-a66a-1a18bcfb5aff")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        OrderKind OrderKind { get; set; }

        #region Allors
        [Id("c6f86f31-d254-4001-94fa-273d041df31a")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("6145aff7-c0dd-4441-927b-f35857a8f225")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("3182b8cf-477c-47fc-84c5-93ea78edcc7d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("3193d806-ec3e-4abd-bab6-2b2c10c43c69")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("e039e94d-db89-4a17-a692-e82fdb53bfea")]
        #endregion
        [Workspace(Default)]
        [Required]
        DateTime OrderDate { get; set; }

        #region Allors
        [Id("f38b3c7d-ac20-49be-a115-d7e83557f49a")]
        #endregion
        [Workspace(Default)]
        DateTime DeliveryDate { get; set; }

        #region Allors
        [Id("6f0c38b9-8f08-4eb4-8b99-9596121a75a1")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableOrderNumber { get; set; }

        #region Allors
        [Id("7623fb2c-af18-437b-b746-ec5d05c696b2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Media[] ElectronicDocuments { get; set; }

        #region Allors
        [Id("593bd702-0420-498c-be36-8831e657e2aa")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public Locale DefaultLocale { get; set; }

        #region Allors
        [Id("a18c4cd5-e799-45df-a83f-591d3513aaae")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public Currency DefaultCurrency { get; set; }

        #region Allors
        [Id("4374d771-a0ed-4d4f-8acc-18daaa0ee6cb")]
        #endregion
        [Required]
        [Workspace(Default)]
        public Guid DerivationTrigger { get; set; }

        #region Allors

        [Id("14B59435-4304-4070-AA25-EFDAB6431E73")]

        #endregion
        [Workspace(Default)]
        void Create();

        #region Allors
        [Id("116D62FC-04E5-407C-B044-7092454C8806")]
        #endregion
        [Workspace(Default)]
        void Approve();

        #region Allors
        [Id("8e77c337-e0ef-4524-b657-f904baaa8762")]
        #endregion
        [Workspace(Default)]
        void Revise();

        #region Allors
        [Id("F735D397-B989-41E8-A042-5C9EAEB41C32")]
        #endregion
        [Workspace(Default)]
        void Reject();

        #region Allors
        [Id("6ECEF1FD-19A6-44E0-97B9-1D0F879074B4")]
        #endregion
        [Workspace(Default)]
        void Hold();

        #region Allors
        [Id("4F5D213B-C6FC-424A-B8FE-4493B1D4E7B3")]
        #endregion
        [Workspace(Default)]
        void Continue();

        #region Allors
        [Id("6167FF6D-DED4-45BC-B4C4-5955B4727200")]
        #endregion
        [Workspace(Default)]
        void Cancel();

        #region Allors
        [Id("80BF3BC5-25D5-4CF6-A7E9-E01F34AFF9EA")]
        #endregion
        [Workspace(Default)]
        void Complete();

        #region Allors

        [Id("794F36F3-04A0-41E9-8AE1-AD48C006CE6B")]

        #endregion
        [Workspace(Default)]
        void Invoice();

        #region Allors
        [Id("468AA6DB-A42B-4389-AF15-70CA3265FC5E")]
        #endregion
        [Workspace(Default)]
        void Reopen();
    }
}
