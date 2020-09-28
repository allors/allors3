// <copyright file="DesiredProductFeature.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("dda88fe9-14b3-463b-ae66-25dd1b136e16")]
    #endregion
    public partial class DesiredProductFeature : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("24695d7b-5c61-4b5c-be90-0f18ca46c6a6")]
        #endregion
        [Required]

        public bool Required { get; set; }

        #region Allors
        [Id("d09dbd42-5c59-4d78-b5d7-4dbee0406ead")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public ProductFeature ProductFeature { get; set; }

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
