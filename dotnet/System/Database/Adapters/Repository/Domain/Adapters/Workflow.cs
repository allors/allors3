// <copyright file="Workflow.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // The full names of the roles of Work and of Workflow differ only in case: WorkFlowOwner and WorkflowOwner,
    // WorkFlowPart and WorkflowPart.
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
    }
}
