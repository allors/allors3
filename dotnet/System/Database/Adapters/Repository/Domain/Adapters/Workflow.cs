// <copyright file="Workflow.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // The full names of the roles of Work and of Workflow differ only in case: WorkFlowOwner and WorkflowOwner,
    // WorkFlowPart and WorkflowPart, WorkFlowLead and WorkflowLead, WorkFlowMember and WorkflowMember.
    #region Allors
    [Id("1a89c290-07e9-40f8-a556-0191fbcd5e55")]
    #endregion
    public partial interface Workflow : Object
    {
        #region Allors
        [Id("70aec4b0-86ad-4b92-bd5f-cda40fb55776")]
        [Size(256)]
        #endregion
        string Owner { get; set; }

        #region Allors
        [Id("1618f343-406c-4431-9032-bf7fa5305e43")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        Job[] Parts { get; set; }

        #region Allors
        [Id("788b24dd-d6fd-487e-b6bf-e8cbcbf7742e")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        Job Lead { get; set; }

        #region Allors
        [Id("2f1c74a8-e28e-4173-8c50-aedd06d3e9b5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        Person[] Members { get; set; }
    }
}
