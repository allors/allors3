// <copyright file="JournalVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("965c7f21-5b39-442e-be74-b8bdb370fb96")]
    #endregion
    public partial class JournalVersion : Version
    {
        #region inherited properties

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("ed04dda1-935b-4112-ac06-c543ec481b5b")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public GeneralLedgerAccount ContraAccount { get; set; }

        #region Allors
        [Id("808e747b-3ebd-4300-9e11-4c3200d10d70")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public JournalType JournalType { get; set; }

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
