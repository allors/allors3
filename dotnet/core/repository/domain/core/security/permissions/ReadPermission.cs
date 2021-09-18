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
    [Id("0716C285-841C-419B-A8C4-A67BFA585CDA")]
    #endregion
    public partial class ReadPermission : Permission
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid ClassPointer { get; set; }

        #endregion

        #region Allors
        [Id("88A27D41-E97E-4446-86D7-2E2FC10C5004")]
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
