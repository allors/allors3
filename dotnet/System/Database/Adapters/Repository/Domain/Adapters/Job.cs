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

        #region Allors
        [Id("bdf83fb6-4b5b-4721-8032-3b95421e914e")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Alias2[] Items { get; set; }

        // Named has two classes, so each relation has its own table. The 61-byte table names differ at byte 59.
        // Inserting Where after Job makes the association names equal through byte 63.
        #region Allors
        [Id("9c5f03ac-e410-47a7-a307-65a9f57f5594")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Named NamedWithALongRoleNameWhoseAssociationNameEndsInTheWordOne { get; set; }

        #region Allors
        [Id("749aa645-38f4-4167-b45b-8fca8d846fbe")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Named NamedWithALongRoleNameWhoseAssociationNameEndsInTheWordTwo { get; set; }

        #region inherited methods
        #endregion

    }
}
