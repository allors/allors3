// <copyright file="Good.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7D9E6DE4-E73D-42BE-B658-729309129A53")]
    #endregion
    public partial interface Good : Product
    {
        #region Allors
        [Id("4e8eceff-aec2-44f8-9820-4e417ed904c1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string BarCode { get; set; }

        #region Allors
        [Id("DDECD426-40C7-4D17-A225-2C46B47F0C89")]
        #endregion
        [Workspace(Default)]
        public decimal ReplacementValue { get; set; }

        #region Allors
        [Id("E25F1487-6F08-4DC5-9838-BAE4FF990ADA")]
        #endregion
        [Workspace(Default)]
        public int LifeTime { get; set; }

        #region Allors
        [Id("D96B2474-B8AE-40F4-9D86-4CA09E2B6965")]
        #endregion
        [Workspace(Default)]
        public int DepreciationYears { get; set; }

        #region Allors
        [Id("acbe2dc6-63ad-4910-9752-4cab83e24afb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Product[] ProductSubstitutions { get; set; }

        #region Allors
        [Id("e1ee15a9-f173-4d81-a11d-82abff076fb4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Product[] ProductIncompatibilities { get; set; }
    }
}
