// <copyright file="NonSerializedInventoryItemStatus.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("700360b9-56be-4e51-9610-f1e5951dd765")]
    #endregion
    public partial class NonSerialisedInventoryItemStatus : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("590f1d9b-a805-4b0e-a2bd-8e274608fe3c")]
        #endregion
        [Required]

        public DateTime StartDateTime { get; set; }

        #region Allors
        [Id("959aa0a9-a197-4eb4-bc9e-e40da8892dd0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public NonSerialisedInventoryItemObjectState NonSerialisedInventoryItemObjectState { get; set; }

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
