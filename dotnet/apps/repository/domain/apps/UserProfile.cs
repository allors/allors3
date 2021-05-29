// <copyright file="UserProfile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("EF12372D-2A94-406C-AE3F-18E9CF2FD016")]
    #endregion
    public partial class UserProfile : Deletable
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("5EF420B6-6BA6-4913-87F3-D5EBC40CB794")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation DefaultInternalOrganization { get; set; }

        #region Allors
        [Id("18788ff9-2806-488f-a83d-60feefb40ca3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Locale DefaulLocale { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
