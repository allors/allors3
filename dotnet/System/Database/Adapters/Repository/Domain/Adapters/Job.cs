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

        public Job FlowLead { get; set; }

        public Person[] FlowMembers { get; set; }

        public string Owner { get; set; }

        public Job[] Parts { get; set; }

        public Job Lead { get; set; }

        public Person[] Members { get; set; }

        #endregion

        #region Allors
        [Id("bdf83fb6-4b5b-4721-8032-3b95421e914e")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Alias2[] Items { get; set; }

        // Named has two classes, so each relation has its own table. The full names of the roles have 61 bytes and
        // differ at byte 59. Inserting Where after Job makes the association names equal through byte 63.
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

        // Six roles whose names are equal in their first 128 characters: a unit role, a one-to-many role, whose
        // association is a column of the table of Job, and a many-to-many role, which has a table of its own, two
        // of each. PostgreSQL keeps 63 bytes of a name and SQL Server allows 128 characters.
        #region Allors
        [Id("3ada1f5c-b43b-43bf-91dc-e75d0824ae14")]
        [Size(256)]
        #endregion
        public string WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInFirstText { get; set; }

        #region Allors
        [Id("83f7208d-8753-4132-b07f-436f4400d336")]
        [Size(256)]
        #endregion
        public string WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInSecondText { get; set; }

        #region Allors
        [Id("fc48dbb6-97a1-4a58-9d8e-bafda8a25926")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Job[] WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInFirstParts { get; set; }

        #region Allors
        [Id("c213e71b-44db-4a80-bb91-f61b90bb7fd5")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public Job[] WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInSecondParts { get; set; }

        #region Allors
        [Id("def8467e-1617-434b-9be1-bd027d93c292")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Person[] WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInFirstMembers { get; set; }

        #region Allors
        [Id("d1fa30e2-aa74-4bbc-8b07-e801e67cfe9f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Person[] WithANameBeyondTheSixtyThreeBytesThatPostgreSqlKeepsAndBeyondTheOneHundredAndTwentyEightCharactersThatSqlServerAllowsWhichEndsInSecondMembers { get; set; }

        #region inherited methods
        #endregion

    }
}
