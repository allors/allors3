// <copyright file="JournalEntry.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("11d75a7a-2e86-4430-a6af-2916440c9ecb")]
    #endregion
    public partial class JournalEntry : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("09202ffd-6b78-455b-a140-a354a771d761")]
        #endregion
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("1452d159-857a-4fff-b1d6-6d27772e54bc")]
        #endregion

        public int EntryNumber { get; set; }

        #region Allors
        [Id("1b5f8acd-872d-498e-9c2d-ded4b7d31efe")]
        #endregion

        public DateTime EntryDate { get; set; }

        #region Allors
        [Id("4eca8284-cc27-4440-8b5f-adeffd3b078b")]
        #endregion

        public DateTime JournalDate { get; set; }

        #region Allors
        [Id("e81fe73b-1486-4a9d-ab2b-2d49dfcbb777")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public JournalEntryDetail[] JournalEntryDetails { get; set; }

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
