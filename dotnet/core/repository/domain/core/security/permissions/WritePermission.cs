// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("4F00E50D-4324-4005-A405-6DFD1232982A")]
    #endregion
    public partial class WritePermission : Permission
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid ClassPointer { get; set; }

        #endregion

        #region Allors
        [Id("86675DEA-D9F0-4930-99EC-13F2137CFB45")]
        [Indexed]
        #endregion
        [Required]
        public Guid RelationTypePointer { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }


        #endregion
    }
}
