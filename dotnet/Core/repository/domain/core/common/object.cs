// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("12504f04-02c6-4778-98fe-04eba12ef8b2")]
    #endregion
    public partial interface Object
    {
        #region Allors
        [Id("b816fccd-08e0-46e0-a49c-7213c3604416")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        SecurityToken[] SecurityTokens { get; set; }

        #region Allors
        [Id("E989F7D2-A4AC-43D8-AC7C-CBCDA2CFB6D3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Derived]
        Revocation[] Revocations { get; set; }

        #region Allors
        [Id("FDD32313-CF62-4166-9167-EF90BE3A3C75")]
        #endregion
        void OnBuild();

        #region Allors
        [Id("2B827E22-155D-4AA8-BA9F-46A64D7C79C8")]
        #endregion
        void OnPostBuild();

        #region Allors
        [Id("4E5A4C91-C430-49FB-B15D-D4CB0155C551")]
        #endregion
        void OnInit();

        #region Allors
        [Id("07AFF35D-F4CB-48FE-A39A-176B1931FAB7")]
        #endregion
        void OnPostDerive();
    }
}
