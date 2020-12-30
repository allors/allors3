// <copyright file="Store.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d8611e48-b0ba-4037-a992-09e3e26c6d5d")]
    #endregion
    public partial class Store : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("A89BD354-9747-44AF-99E4-054F1CC42D9C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("5A052D80-CD0D-4C95-9281-4DD59A3BE26B")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public Catalogue[] Catalogues { get; set; }

        #region Allors
        [Id("0a0ad3b1-afa2-4c78-8414-e657fabebb3e")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal ShipmentThreshold { get; set; }

        #region Allors
        [Id("3e378f04-0d14-4b03-b8e2-b58da3039184")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string SalesInvoiceNumberPrefix { get; set; }

        #region Allors
        [Id("8a3d0121-e5f9-4bc9-a829-340e1b4b5402")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public Counter SalesInvoiceNumberCounter { get; set; }

        #region Allors
        [Id("22CB75F8-CDF0-4D20-ABF5-79B43ADA30FD")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string CreditNoteNumberPrefix { get; set; }

        #region Allors
        [Id("8193E342-DB5B-4001-8321-3013CEB469EB")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public Counter CreditNoteNumberCounter { get; set; }

        #region Allors
        [Id("e00e948e-6fc3-43fd-a49b-008fc6d6133f")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string SalesOrderNumberPrefix { get; set; }

        #region Allors
        [Id("124a58f1-f7a3-43d1-8f4d-0a068b7a2659")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]

        public Counter SalesOrderNumberCounter { get; set; }

        #region Allors
        [Id("3a837bae-993a-4765-8d4f-b690bf65dc79")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string OutgoingShipmentNumberPrefix { get; set; }

        #region Allors
        [Id("dfc3f6be-0a95-49e0-8742-3901dbab5185")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public Counter OutgoingShipmentNumberCounter { get; set; }

        #region Allors
        [Id("3CBCF813-7FD4-4C69-98C8-CFC260234477")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public Counter SalesInvoiceTemporaryCounter { get; set; }

        #region Allors
        [Id("4927a65d-a9d3-4fad-afce-1ec8679d3a55")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int PaymentGracePeriod { get; set; }

        #region Allors
        [Id("4a647ddb-9a17-4544-8cae-6204140c413a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media LogoImage { get; set; }

        #region Allors
        [Id("555c3b9a-7556-4fdf-a431-6d18a6ae7cbd")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int PaymentNetDays { get; set; }

        #region Allors
        [Id("63d433b9-8cb3-428b-b516-be25f1895673")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility DefaultFacility { get; set; }

        #region Allors
        [Id("6e4b701a-2540-4cec-8413-50bfb69d3a7c")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("79244ed7-6388-48ca-86db-7b57a64fe680")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal CreditLimit { get; set; }

        #region Allors
        [Id("7c9cda07-5920-4037-b934-5b74355c4b85")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ShipmentMethod DefaultShipmentMethod { get; set; }

        #region Allors
        [Id("80670a7a-1be8-4407-917e-fa359e632519")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Carrier DefaultCarrier { get; set; }

        #region Allors
        [Id("954d4e3c-f188-45f4-98b8-ece14ac7dabd")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal OrderThreshold { get; set; }

        #region Allors
        [Id("9a0dfe33-016a-4b41-979c-d17a6f87d2d2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public PaymentMethod DefaultCollectionMethod { get; set; }

        #region Allors
        [Id("bc11d48f-bcab-4880-afe8-0a52d3c11e44")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public FiscalYearStoreSequenceNumbers[] FiscalYearsStoreSequenceNumbers { get; set; }

        #region Allors
        [Id("ca82d0f8-f886-4936-80f5-a7dbb7c550b5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod[] CollectionMethods { get; set; }

        #region Allors
        [Id("85279191-9836-444B-A5CB-742A488D0467")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public BillingProcess BillingProcess { get; set; }

        #region Allors
        [Id("ECA0A308-BB12-419C-8E10-67BDCC7D37E6")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsImmediatelyPicked { get; set; }

        #region Allors
        [Id("7A014C3C-F593-4528-AE32-EE4BE55D76A4")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsImmediatelyPacked { get; set; }

        #region Allors
        [Id("1136BB3C-905C-411B-AFED-FBE04BE132BD")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsAutomaticallyShipped { get; set; }

        #region Allors
        [Id("8D639C6A-B8C1-4FFA-867F-95B75B4A6807")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool AutoGenerateCustomerShipment { get; set; }

        #region Allors
        [Id("B3E6A681-E883-4FD5-82E4-F5A94F3F5148")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool AutoGenerateShipmentPackage { get; set; }

        #region Allors
        [Id("CE31A755-7053-4A27-A0AE-7C38AFA03E2F")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseCreditNoteSequence { get; set; }

        #region Allors
        [Id("3CECAB0B-A766-474E-9DB1-D6B93E02FF41")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool SerialisedInventoryItemStore { get; set; }

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
