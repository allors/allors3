// <copyright file="Job.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("31a7013d-b29c-4cd3-b286-d623b58d300b")]
    #endregion
    public partial class Job : System.Object, Work, Workflow
    {
        #region inherited properties
        public string FlowOwner { get; set; }

        public Job[] FlowParts { get; set; }

        public string Owner { get; set; }

        public Job[] Parts { get; set; }

        #endregion

        // The full name of this role, the name of its class followed by its own, has 61 characters, and Named has
        // two classes, so the relation has a table of its own.
        #region Allors
        [Id("9c5f03ac-e410-47a7-a307-65a9f57f5594")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Named NamedWithARoleNameThatGivesThisRoleAFullNameOf61Characters { get; set; }

        #region inherited methods
        #endregion

    }
}
