// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("0716C285-841C-419B-A8C4-A67BFA585CDA")]
    #endregion
    public partial class RoleReadPermission : Permission
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid ConcreteClassPointer { get; set; }

        #endregion

        #region Allors
        [Id("88A27D41-E97E-4446-86D7-2E2FC10C5004")]
        [AssociationId("8989DD3B-95A1-4E5B-9B97-A51241AB1AAC")]
        [RoleId("1DED75BF-426E-43B9-83EF-C425FAB24A54")]
        [Indexed]
        #endregion
        [Required]
        public Guid RelationTypePointer { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
