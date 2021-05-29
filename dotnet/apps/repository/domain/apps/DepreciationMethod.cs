// <copyright file="DepreciationMethod.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("63ca0535-95e5-4b2d-847d-d619a5365605")]
    #endregion
    public partial class DepreciationMethod : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("a87fd42b-7be3-4cd4-9393-64b1cf03c050")]
        #endregion
        [Required]
        [Size(256)]

        public string Formula { get; set; }

        #region Allors
        [Id("b0a81d90-f6bc-4169-b76c-497a3a1f04bf")]
        #endregion
        [Size(-1)]

        public string Description { get; set; }

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
