// <copyright file="JournalVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("128c9089-cafe-4861-a101-ccefd4b93c46")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public OrganisationGlAccount ContraAccount { get; set; }

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

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
