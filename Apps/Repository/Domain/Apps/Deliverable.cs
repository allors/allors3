// <copyright file="Deliverable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("68a6803d-0e65-4141-ac51-25f4c2e49914")]
    #endregion
    public partial class Deliverable : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("d7322009-e68f-4635-bc0e-1c0b5a46de62")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

        #region Allors
        [Id("dfd5fb95-50ee-48a5-942b-75752f78a615")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public DeliverableType DeliverableType { get; set; }

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
