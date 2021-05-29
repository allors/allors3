// <copyright file="WorkEffortAssignmentRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("ac18c87b-683c-4529-9171-d23e73c583d4")]
    #endregion
    public partial class WorkEffortAssignmentRate : Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("BFFC696B-84AE-4803-8C80-FF20E99BE46D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("98BF1DDB-F0B7-48B6-9CFA-FD1832B4C0AC")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public DateTime FromDate { get; set; }

        #region Allors
        [Id("651B674C-44AA-41D4-979D-958EBC3FEC5D")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public DateTime ThroughDate { get; set; }

        #region Allors
        [Id("738EFE42-075D-46B6-974C-CD57FFAC7401")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Rate { get; set; }

        #region Allors
        [Id("74A2B45F-4873-434F-900F-D1663B170172")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public RateType RateType { get; set; }

        #region Allors
        [Id("A140FB3E-76E8-411E-835A-FE9EB8E84F19")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("953BAA4D-5455-47C8-B8B0-D3673EFF358D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("627da684-d501-4221-97c2-81329e2b5e8c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public WorkEffortPartyAssignment WorkEffortPartyAssignment { get; set; }

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
