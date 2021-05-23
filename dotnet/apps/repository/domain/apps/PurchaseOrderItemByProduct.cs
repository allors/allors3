// <copyright file="PurchaseOrderItemByProduct.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("e30f5fc7-8d51-4a83-a334-fe07a1b13f63")]
    #endregion
    public partial class PurchaseOrderItemByProduct : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("5b541bdc-966e-4b16-bfea-a53e4eff48f7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Required]
        [Workspace(Default)]
        public UnifiedProduct UnifiedProduct { get; set; }

        #region Allors
        [Id("0e0e4e0e-13bf-4e85-a18d-2404bfb6cd8f")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityOrdered { get; set; }

        #region Allors
        [Id("e78a4438-c873-4991-9ac0-7171864f46f4")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal ValueOrdered { get; set; }

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
