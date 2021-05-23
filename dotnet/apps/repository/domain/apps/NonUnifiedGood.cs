// <copyright file="NonUnifiedGood.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("e3e87d40-b4f0-4953-9716-db13b35d716b")]
    #endregion
    public partial class NonUnifiedGood : Good, Versioned
    {
        #region inherited properties
        public string Comment { get; set; }

        public string ProductNumber { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Name { get; set; }

        public LocalisedText[] LocalisedNames { get; set; }

        public string Description { get; set; }

        public LocalisedText[] LocalisedDescriptions { get; set; }

        public string InternalComment { get; set; }

        public Document[] Documents { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; }

        public string Keywords { get; set; }

        public LocalisedText[] LocalisedKeywords { get; set; }

        public Media PrimaryPhoto { get; set; }

        public Media[] Photos { get; set; }

        public DateTime SupportDiscontinuationDate { get; set; }

        public DateTime SalesDiscontinuationDate { get; set; }

        public PriceComponent[] VirtualProductPriceComponents { get; set; }

        public string IntrastatCode { get; set; }

        public Product ProductComplement { get; set; }

        public Product[] Variants { get; set; }

        public DateTime IntroductionDate { get; set; }

        public EstimatedProductCost[] EstimatedProductCosts { get; set; }

        public Product[] ProductObsolescences { get; set; }

        public VatRegime VatRegime { get; set; }

        public PriceComponent[] BasePrices { get; set; }

        public ProductIdentification[] ProductIdentifications { get; set; }

        public string BarCode { get; set; }
        public decimal ReplacementValue { get; set; }
        public int LifeTime { get; set; }
        public int DepreciationYears { get; set; }

        public Product[] ProductSubstitutions { get; set; }

        public Product[] ProductIncompatibilities { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string SearchString { get; set; }

        public Media[] PublicElectronicDocuments { get; set; }

        public LocalisedMedia[] PublicLocalisedElectronicDocuments { get; set; }

        public Media[] PrivateElectronicDocuments { get; set; }

        public LocalisedMedia[] PrivateLocalisedElectronicDocuments { get; set; }

        public Scope Scope { get; set; }
        #endregion

        #region Versioning
        #region Allors
        [Id("ecb52be1-da71-4965-aeca-0152ba39eb1a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public NonUnifiedGoodVersion CurrentVersion { get; set; }

        #region Allors
        [Id("2fa67859-6a43-4d58-9823-b1282526b2a9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public NonUnifiedGoodVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("82295ab2-8488-4d7e-8703-9f7fbec55925")]
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

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
