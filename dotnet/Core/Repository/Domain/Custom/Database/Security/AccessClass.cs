// <copyright file="AccessClass.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("8E66F2E1-27FA-4A1C-B410-F082CA1621C7")]
    #endregion
    public partial class AccessClass : AccessInterface
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }

        #endregion


        #region Allors
        [Id("A67189D3-CD06-425B-98BB-59E0E73AC211")]
        #endregion
        public string Property { get; set; }

        #region Allors
        [Id("4DB6E69E-F870-4643-A2F6-D3C700FBB431")]
        #endregion
        public string AnotherProperty { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion
    }
}
