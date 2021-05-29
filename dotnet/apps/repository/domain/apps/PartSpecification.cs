// <copyright file="PartSpecification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0091574c-edac-4376-8d03-c7e2c2d8132f")]
    #endregion
    public partial class PartSpecification : UniquelyIdentifiable, Commentable, Transitional, Versioned
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region ObjectStates
        #region
        #region Allors
        [Id("99C34A9A-D41A-415C-A4B7-2D1931451A68")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PartSpecificationState PreviousPartSpecificationState { get; set; }

        #region Allors
        [Id("41EBF5DA-B98C-4588-84E5-8DE87B508716")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PartSpecificationState LastPartSpecificationState { get; set; }

        #region Allors
        [Id("41FAB301-8559-44A2-B47B-7FE00ED1BBCC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PartSpecificationState PartSpecificationState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("425C11EE-048F-4CA3-878B-C158BD6CABCC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PartSpecificationVersion CurrentVersion { get; set; }

        #region Allors
        [Id("2456FCE3-44CD-467E-96EA-BA16E2DE0772")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PartSpecificationVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("6a83ef4b-1ef5-4782-b9fd-19e3231c29c5")]
        #endregion
        public DateTime DocumentationDate { get; set; }

        #region Allors
        [Id("e20b0fd5-f10a-44df-8bef-f454e7d23bce")]
        #endregion
        [Required]
        [Size(-1)]
        public string Description { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

        #region Allors
        [Id("21279E2E-60A0-4B07-9FB3-D49892001DD2")]

        #endregion

        public void Approve()
        {
        }
    }
}
