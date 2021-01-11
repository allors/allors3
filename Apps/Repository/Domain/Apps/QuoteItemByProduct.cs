// <copyright file="QuoteItemByProduct.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("686515dc-1325-465a-b704-69d128675279")]
    #endregion
    public partial class QuoteItemByProduct : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("d622d4e3-620b-472d-afe4-d51fe2255d36")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Required]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("0a83092b-439c-497a-a562-05e4459c3382")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityOrdered { get; set; }

        #region Allors
        [Id("6522c42a-dd5a-4595-89c5-e86f17e6e4dc")]
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
