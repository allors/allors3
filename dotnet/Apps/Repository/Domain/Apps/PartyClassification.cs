// <copyright file="PartyClassification.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3bb83aa5-e58a-4421-bdbc-3c9fa0b2324f")]
    #endregion
    public partial interface PartyClassification : Object
    {
        #region Allors
        [Id("4f35ae7e-fe06-4a3b-abe1-adb78fcf2e6b")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string Name { get; set; }

        #region Allors

        [Id("0058B700-0F45-4EFD-8094-4BE6404BF502")]

        #endregion

        [Workspace(Default)]
        void Delete();
    }
}
