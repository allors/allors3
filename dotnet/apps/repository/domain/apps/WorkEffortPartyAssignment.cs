// <copyright file="WorkEffortPartyAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0bdfb093-35af-4c87-9c1c-05ed9dae6df6")]
    #endregion
    public partial class WorkEffortPartyAssignment : Commentable, Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region Allors
        [Id("3F3D9387-0758-4559-B33F-0C7B352B171C")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public DateTime FromDate { get; set; }

        #region Allors
        [Id("2A49EA68-DB8F-4186-9D7E-FE2CC1AFD6F5")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public DateTime ThroughDate { get; set; }

        #region Allors
        [Id("2723be72-6775-4f39-9bf6-e95abc2c0b24")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

        #region Allors
        [Id("92081ae5-3e2a-4b13-98b8-0fc45403b877")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Party Party { get; set; }

        #region Allors
        [Id("f88ae79d-7605-4be9-972e-541489bdb72b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region Allors
        [Id("A5E053FB-957D-49A4-8EFD-D918043BEDBA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public WorkEffortAssignmentRate[] AssignmentRates { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }

        #endregion
    }
}
