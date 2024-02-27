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
    public partial class WorkEffortPartyAssignment : Commentable, Deletable, DelegatedAccessObject, Period
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

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

        #region Workspace
        #region Allors
        [Id("20017ae5-5c42-4909-a9c0-8b8ac42b8d48")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string DisplayName { get; set; }
        #endregion

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
