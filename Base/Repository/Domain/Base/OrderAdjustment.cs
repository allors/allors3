// <copyright file="OrderAdjustment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("c5578565-c07a-4dc1-8381-41955db364e2")]
    #endregion
    public partial interface OrderAdjustment : Deletable, Versioned
    {
        #region Allors
        [Id("4e7cbdda-9f19-44dd-bbef-6cab5d92a8a3")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal Amount { get; set; }

        #region Allors
        [Id("bc1ad594-88b6-4176-994c-a52be672f06d")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal Percentage { get; set; }

        #region Allors
        [Id("9f563bb3-36c2-4d9f-9520-dc725d32b2e8")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }
    }
}
