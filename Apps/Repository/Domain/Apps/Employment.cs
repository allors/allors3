// <copyright file="Employment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("6a7e45b2-36b2-4d2e-a29c-0cc13851766f")]
    #endregion
    public partial class Employment : PartyRelationship, Period, Deletable, Object
    {
        #region inherited properties

        public Party[] Parties { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Agreement[] Agreements { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("a243feb0-e5f0-41b4-9b13-a09bb8413fb3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Person Employee { get; set; }

        #region Allors
        [Id("CED5A46F-0934-41A7-B2B1-8BFFD152C94D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation Employer { get; set; }

        #region Allors
        [Id("8ED07003-CAAC-4A4F-A948-5992CAF8FBBC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PayrollPreference[] PayrollPreferences { get; set; }

        #region Allors
        [Id("c8fd6c79-f909-414e-b9e3-5e911e2e2080")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public EmploymentTerminationReason EmploymentTerminationReason { get; set; }

        #region Allors
        [Id("e79807d4-dcf8-47e2-b510-e8535f1ec436")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public EmploymentTermination EmploymentTermination { get; set; }

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
