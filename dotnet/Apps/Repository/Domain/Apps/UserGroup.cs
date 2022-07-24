// <copyright file="UserGroup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    public partial class UserGroup
    {
        #region Allors
        [Id("472e2bd2-40d4-4924-a9b1-57566b792f6a")]
        #endregion
        [Required]
        [Workspace]
        public bool isSelectable{ get; set; }

        #region Allors
        [Id("016fbfe7-5ac3-41b3-96b4-f5badaeee611")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace()]
        public User[] InMembers { get; set; }

        #region Allors
        [Id("b8be9146-b24e-4dec-9969-2b7149f9564a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace()]
        public User[] OutMembers { get; set; }
    }
}
