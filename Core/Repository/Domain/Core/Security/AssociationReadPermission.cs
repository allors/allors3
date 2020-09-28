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
    [Id("5FCC8E66-A011-494E-BEFC-CDDE4BEFA144")]
    #endregion
    public partial class AssociationReadPermission : Permission
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid ConcreteClassPointer { get; set; }

        #endregion
        
        #region Allors
        [Id("45F604A0-E451-4E83-BE9D-23625929604A")]
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
