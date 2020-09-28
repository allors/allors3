// <copyright file="ProductIdentification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("55DE0F4F-2ABD-4943-8319-39DC5D51B0D7")]
    #endregion
    public partial interface ProductIdentification : Deletable, Object
    {
        #region Allors
        [Id("80CE30EE-71CF-4E74-8D40-C0BCD9239A9C")]
        #endregion
        [Required]
        [Workspace(Default)]
        string Identification { get; set; }

        #region Allors
        [Id("2B6445D3-EE83-4C9C-BEAC-867A3E823B9B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        ProductIdentificationType ProductIdentificationType { get; set; }
    }
}
