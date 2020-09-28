// <copyright file="Case.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("a0705b81-2eef-4c51-9454-a31bcedc20a3")]
    #endregion
    public partial class Case : Transitional, UniquelyIdentifiable, Versioned
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region ObjectStates
        #region CaseState
        #region Allors
        [Id("8865A76F-2D5F-425A-8F28-CC17E8280D6E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public CaseState PreviousCaseState { get; set; }

        #region Allors
        [Id("BEFFB894-CC36-40F5-9BEE-2DC71EB28434")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public CaseState LastCaseState { get; set; }

        #region Allors
        [Id("5DB5AE45-14A7-4CB0-9634-E2F347D07F0E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public CaseState CaseState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("FDEF4590-CE75-4E71-AB15-4142CDD44C42")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public CaseVersion CurrentVersion { get; set; }

        #region Allors
        [Id("3DDE0045-7D6F-49AF-9B92-7BE3FE6EA05C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public CaseVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("65e310b5-1358-450c-aec2-985dcc724cdd")]
        #endregion
        public DateTime StartDate { get; set; }

        #region Allors
        [Id("87f64957-53f9-446e-ac1f-323a00da027f")]
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

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
