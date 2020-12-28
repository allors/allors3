// <copyright file="SalesOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("f3343263-f4bf-4abd-9b34-1a3426ed4f10")]
    #endregion
    public partial class SalesOrderItemByProduct : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("74f9ba11-d14d-4c76-8d7e-5489f74c2274")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Required]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("772acaea-eea6-495b-b23e-fdc99899c3da")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityOrdered { get; set; }

        #region Allors
        [Id("6a3428c7-d2f7-4f44-bc64-f72e91f9bfb2")]
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
