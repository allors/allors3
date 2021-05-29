// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("81C11FD0-E121-4AE7-B624-441968B62089")]
        #endregion
        [Required]
        public bool Block { get; set; }

        #region Allors
        [Id("A67189D3-CD06-425B-98BB-59E0E73AC211")]
        #endregion
        public string Property { get; set; }

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

        public void DelegateAccess()
        {
        }
        #endregion
    }
}
