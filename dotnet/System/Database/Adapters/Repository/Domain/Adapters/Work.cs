// <copyright file="Work.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // The full names of the roles of Work and of Workflow differ only in case: WorkFlowOwner and WorkflowOwner,
    // WorkFlowPart and WorkflowPart, WorkFlowLead and WorkflowLead, WorkFlowMember and WorkflowMember.
    #region Allors
    [Id("30d45658-7ce4-4733-b965-63897562cec2")]
    #endregion
    public partial interface Work : Object
    {
        #region Allors
        [Id("ed9cb638-553c-4018-a01e-ff73821a8e26")]
        [Size(256)]
        #endregion
        string FlowOwner { get; set; }

        #region Allors
        [Id("1e8db9ff-e92a-4597-abc2-6ef541dcd863")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        Job[] FlowParts { get; set; }

        #region Allors
        [Id("174f146d-789d-4428-a632-1acd3ad93537")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        Job FlowLead { get; set; }

        #region Allors
        [Id("c4c2e174-b03c-40a1-9bf0-5c84a4be82f4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        Person[] FlowMembers { get; set; }
    }
}
