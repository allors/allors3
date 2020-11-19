// <copyright file="Engagement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("752a68b0-836e-4cd5-92d5-ebf2bfeda491")]
    #endregion
    public partial class Engagement : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("135792e4-42ad-4bf5-914b-ffc154330cd1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Agreement Agreement { get; set; }

        #region Allors
        [Id("1fb112f4-9628-40a8-9531-2d9ad24103ff")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public ContactMechanism PlacingContactMechanism { get; set; }

        #region Allors
        [Id("2e703224-c40a-45ee-8703-cafe11eda70a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal MaximumAmount { get; set; }

        #region Allors
        [Id("4afffb12-3a70-4903-af99-ed814fd6a444")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public ContactMechanism BillToContactMechanism { get; set; }

        #region Allors
        [Id("55102c87-3cea-4c53-bafb-eb94bdda2b44")]
        #endregion
        [Required]
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("5b758dbc-11f6-4ac4-8f2b-639937814cae")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Party BillToParty { get; set; }

        #region Allors
        [Id("5f524932-4787-4bb4-8785-9a4f67dbb6ed")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Party PlacingParty { get; set; }

        #region Allors
        [Id("6ca9444e-3e1c-4631-ad4e-1025fc85c1a4")]
        #endregion
        public DateTime StartDate { get; set; }

        #region Allors
        [Id("83acc3c0-e87a-48fd-9c10-5070cd7c2a3d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public ContactMechanism TakenViaContactMechanism { get; set; }

        #region Allors
        [Id("9bc7fbff-11dd-434e-91ff-7cef4e225bb3")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedAmount { get; set; }

        #region Allors
        [Id("d9df5d5e-e0cc-4c9e-9e0a-dc5423561774")]
        #endregion
        public DateTime EndDate { get; set; }

        #region Allors
        [Id("e1081976-b7e4-4e8e-85de-bd6ff096b39b")]
        #endregion
        public DateTime ContractDate { get; set; }

        #region Allors
        [Id("ec51db38-d0de-430d-82cc-1105f336977b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public EngagementItem[] EngagementItems { get; set; }

        #region Allors
        [Id("f444a4cf-9722-420b-8ba1-0591492929e5")]
        #endregion
        [Size(256)]
        public string ClientPurchaseOrderNumber { get; set; }

        #region Allors
        [Id("f7e10109-a4f2-4a54-b810-b00edf8a9330")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public OrganisationContactRelationship TakenViaOrganisationContactRelationship { get; set; }

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
