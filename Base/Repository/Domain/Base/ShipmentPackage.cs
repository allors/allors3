// <copyright file="ShipmentPackage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("444e431b-f078-46e0-9c8e-694e15e807c7")]
    #endregion
    public partial class ShipmentPackage : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("293eb102-b098-4e5d-8cef-d5e0b4f1ca5d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public PackagingContent[] PackagingContents { get; set; }

        #region Allors
        [Id("7f009302-d4f4-4b06-9e18-fb1c35bd79e7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public Document[] Documents { get; set; }

        #region Allors
        [Id("afd7e182-d201-4eee-803c-9fb4dff0feed")]
        #endregion
        [Required]

        public DateTime CreationDate { get; set; }

        #region Allors
        [Id("d767222a-b528-4a3f-ac3f-333de19f7ae1")]
        #endregion
        [Derived]
        [Required]

        public int SequenceNumber { get; set; }

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
