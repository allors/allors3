// <copyright file="ProductConfiguration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("23503dae-02ff-4dae-950e-d699dcb12a3c")]
    #endregion
    public partial class ProductConfiguration : ProductAssociation
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("463f9523-62e0-4f33-a0fd-29b42f4af046")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]

        public Product[] ProductsUsedIn { get; set; }

        #region Allors
        [Id("528afcdf-09c2-4b3a-89b0-4da8fd732e83")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Product Product { get; set; }

        #region Allors
        [Id("9e6d3782-2f32-4155-a1bf-62c02e8cbe82")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityUsed { get; set; }

        #region Allors
        [Id("caabfae5-6cff-41df-a267-9f4bde0b4808")]
        #endregion
        [Required]
        [Size(-1)]

        public string Description { get; set; }

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
