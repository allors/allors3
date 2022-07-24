// <copyright file="UserGroup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("60065f5d-a3c2-4418-880d-1026ab607319")]
    #endregion
    public partial class UserGroup : UniquelyIdentifiable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("585bb5cf-9ba4-4865-9027-3667185abc4f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public User[] Members { get; set; }

        #region Allors
        [Id("e94e7f05-78bd-4291-923f-38f82d00e3f4")]
        #endregion
        [Indexed]
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

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
