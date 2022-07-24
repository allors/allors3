// <copyright file="TaskAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("4092d0b4-c6f4-4b81-b023-66be3f4c90bd")]
    #endregion
    [Workspace(Default)]
    public partial class TaskAssignment : Deletable, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("c32c19f1-3f41-4d11-b19d-b8b2aa360166")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Required]
        public User User { get; set; }

        #region Allors
        [Id("f4e05932-89c0-4f40-b4b2-f241ac42d8a0")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        #endregion
        public Notification Notification { get; set; }

        #region Allors
        [Id("8a01f221-480f-4d61-9a12-72e3689a8224")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Required]
        public Task Task { get; set; }

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
