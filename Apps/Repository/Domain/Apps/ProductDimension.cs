// <copyright file="ProductDimension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("C3A647C2-1073-4D8B-99EB-AE5293AADB6B")]
    #endregion
    public partial class ProductDimension : ProductFeature, Deletable
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public EstimatedProductCost[] EstimatedProductCosts { get; set; }

        public PriceComponent[] BasePrices { get; set; }

        public string Description { get; set; }

        public ProductFeature[] DependentFeatures { get; set; }

        public ProductFeature[] IncompatibleFeatures { get; set; }

        public VatRate VatRate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("8E02D67A-1033-4F19-B67B-6B58A000494C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Dimension Dimension { get; set; }

        #region Allors
        [Id("21E9C310-4C2F-4B51-8D8B-24FFAA00FE0F")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Size(-1)]
        [Workspace(Default)]
        public string Value { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPreDerive()
        {
        }

        public void OnDerive()
        {
        }

        public void OnPostDerive()
        {
        }

        public void Delete()
        {
        }

        #endregion
    }
}
