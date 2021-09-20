// <copyright file="Order.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("94be4938-77c1-488f-b116-6d4daeffcc8d")]
    #endregion
    public partial class Order : Transitional, Versioned
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public Revocation[] TransitionalRevocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }
        #endregion

        #region ObjectStates
        #region OrderState
        #region Allors
        [Id("7CFAFE73-FEBD-4BFF-B42F-BE9ECF9E74DD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public OrderState PreviousOrderState { get; set; }

        #region Allors
        [Id("427C6D78-2272-4069-A326-2F551812D6BD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public OrderState LastOrderState { get; set; }

        #region Allors
        [Id("B11EBAC9-5867-4A96-A59B-8A160614FFD6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public OrderState OrderState { get; set; }
        #endregion

        #region ShipmentState
        #region Allors
        [Id("412BACF5-F927-42D0-BE29-F2870768FA76")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public ShipmentState PreviousShipmentState { get; set; }

        #region Allors
        [Id("6C724955-90CA-4069-ACF0-E2A228A928AD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public ShipmentState LastShipmentState { get; set; }

        #region Allors
        [Id("5FEE0701-6C67-478D-9763-25E1E1C70BA1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public ShipmentState ShipmentState { get; set; }
        #endregion

        #region PaymentState
        #region Allors
        [Id("45981825-4E17-440A-9F60-9DE93DBCA7D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PaymentState PreviousPaymentState { get; set; }

        #region Allors
        [Id("4E56EDF6-F45F-4CEC-8BDA-28536490503A")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PaymentState LastPaymentState { get; set; }

        #region Allors
        [Id("BB076A8A-D2E6-47FA-A334-08B0E7E89F05")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public PaymentState PaymentState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("4819AB04-B36F-42F8-B6DE-1F15FFC65233")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public OrderLine[] OrderLines { get; set; }

        #region Allors
        [Id("5aa7fa5c-c0a5-4384-9b24-9ecef17c4848")]
        #endregion
        public decimal Amount { get; set; }

        #region Allors
        [Id("B8F02B30-51A3-44CD-85A3-1E1E13DBC0A4")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public OrderState NonVersionedCurrentObjectState { get; set; }

        #region Allors
        [Id("1879ABB2-78D9-40AF-B404-6CEEF76C7EEC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public OrderLine[] NonVersionedOrderLines { get; set; }

        #region Allors
        [Id("D237EF03-A748-4A89-A009-40D73EFBE9AA")]
        #endregion
        public decimal NonVersionedAmount { get; set; }

        #region Versioning
        #region Allors
        [Id("4058FCBA-9323-47C5-B165-A3EED8DE70B6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public OrderVersion CurrentVersion { get; set; }

        #region Allors
        [Id("DF0E52D4-07B3-45AC-9F36-2C0DE9802C2F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public OrderVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
