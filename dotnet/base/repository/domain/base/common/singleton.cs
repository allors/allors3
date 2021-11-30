// <copyright file="Singleton.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("313b97a5-328c-4600-9dd2-b5bc146fb13b")]
    #endregion
    public partial class Singleton : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9c1634ab-be99-4504-8690-ed4b39fec5bc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public Locale DefaultLocale { get; set; }

        #region Allors
        [Id("9e5a3413-ed33-474f-adf2-149ad5a80719")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Locale[] AdditionalLocales { get; set; }

        #region Allors
        [Id("615AC72B-B3DF-4057-9B7C-C8528341F5FE")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public Locale[] Locales { get; set; }

        #region Allors
        [Id("B2166062-84DA-449D-B34F-983A0C81BC31")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media LogoImage { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
