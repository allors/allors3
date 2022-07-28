// <copyright file="WorkRequirementVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("B0A09032-FEC1-4047-8264-8DBD68C281A0")]
    #endregion
    public partial class WorkRequirementVersion : RequirementVersion
    {
        #region inherited properties
        public RequirementState RequirementState { get; set; }
        public DateTime RequiredByDate { get; set; }
        public RequirementType RequirementType { get; set; }
        public Party Authorizer { get; set; }
        public string Reason { get; set; }
        public Requirement[] Children { get; set; }
        public Party NeededFor { get; set; }
        public Party Originator { get; set; }
        public Priority Priority { get; set; }
        public Facility Facility { get; set; }
        public Organisation ServicedBy { get; set; }
        public decimal EstimatedBudget { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("102a2b1c-6d49-4091-965b-3029ff79c9d0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public FixedAsset FixedAsset { get; set; }

        #region Allors
        [Id("11ef6710-9563-42b3-b4f5-239ced01b1fd")]
        #endregion
        [Workspace(Default)]
        public string Location { get; set; }

        #region Allors
        [Id("a4a2fbc9-f91d-4768-86b5-3b4947f274d3")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public bool UnServiceable { get; set; }

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
