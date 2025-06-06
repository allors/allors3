// <copyright file="SerializedInventoryItemStatus.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("1da3e549-47cb-4896-94ec-3f8a263bb559")]
    #endregion
    public partial class SerialisedInventoryItemStatus : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("aabb931a-38ee-4568-af8c-5f8ed98ed7b9")]
        #endregion
        [Required]

        public DateTime StartDateTime { get; set; }

        #region Allors
        [Id("d2c2fff8-73ec-4748-9c8f-29304abbdb0d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public SerialisedInventoryItemObjectState SerialisedInventoryItemObjectState { get; set; }

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
