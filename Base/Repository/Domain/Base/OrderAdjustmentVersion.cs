// <copyright file="OrderAdjustmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("A5627D2B-3E75-41B7-86BB-642C12714471")]
    #endregion
    public partial interface OrderAdjustmentVersion : Version
    {
        #region Allors
        [Id("F894196C-EF00-473B-BC09-EDDDFE5500CF")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal Amount { get; set; }

        #region Allors
        [Id("C9703601-2F24-4D5D-83B6-14411D872435")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal Percentage { get; set; }
    }
}
