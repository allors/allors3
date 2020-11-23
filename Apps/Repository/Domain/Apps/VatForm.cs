// <copyright file="VatForm.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("eba70b57-05e3-487f-8cf1-45f14fcdc3b9")]
    #endregion
    public partial class VatForm : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("180f9887-5973-4c4a-9277-a383e4f66bc6")]
        #endregion
        [Size(256)]

        public string Name { get; set; }

        #region Allors
        [Id("f3683ece-e247-490f-be4f-4fe12e5cfd06")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public VatReturnBox[] VatReturnBoxes { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}
