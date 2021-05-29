// <copyright file="SupplierOffering.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0ae3caca-9b4b-407f-bd98-46db03b72a43")]
    #endregion
    public partial class SupplierOffering : Commentable, Period, Deletable, Versioned
    {
        #region inherited properties
        public string Comment { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("cf26c1a3-b696-464c-a9b8-0c2e142f992e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SupplierOfferingVersion CurrentVersion { get; set; }

        #region Allors
        [Id("f0e79f83-1698-4550-a7f9-9f0d69c4a087")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SupplierOfferingVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("44e38ad4-833c-4da9-894d-bbe57d0f784e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public RatingType Rating { get; set; }

        #region Allors
        [Id("74895df9-e416-41cb-ab36-24694dc63334")]
        #endregion
        [Workspace(Default)]
        public int StandardLeadTime { get; set; }

        #region Allors
        [Id("a59d91cc-610f-46b6-8935-e95a42edc31e")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        public decimal Price { get; set; }

        #region Allors
        [Id("aa7af527-e616-4d01-86b4-e116c3087a37")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("c16a7bec-e1fc-4034-8eb7-0223b776db7a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Currency Currency { get; set; }

        #region Allors
        [Id("9c3458aa-7062-4c4c-9160-2f978b088082")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Ordinal Preference { get; set; }

        #region Allors
        [Id("b4cdcc85-583a-49e7-ba35-8985936c7f64")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal MinimalOrderQuantity { get; set; }

        #region Allors
        [Id("459274A7-2A3C-45DF-B1B8-14171A279AE4")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public int QuantityIncrements { get; set; }

        #region Allors
        [Id("d2de1e9e-196f-43d7-903e-566a4858bc02")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Party Supplier { get; set; }

        #region Allors
        [Id("d741765d-d17e-4e6a-88fd-9eee70c82bcf")]
        #endregion
        [Workspace(Default)]
        public string SupplierProductName { get; set; }

        #region Allors
        [Id("E7CBF9F3-9762-4102-9CAA-78D1B5F1F39C")]
        #endregion
        [Workspace(Default)]
        public string SupplierProductId { get; set; }

        #region Allors
        [Id("ea5e3f12-417c-40c4-97e0-d8c7dd41300c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
