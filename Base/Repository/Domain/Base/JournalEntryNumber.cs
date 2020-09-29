// <copyright file="JournalEntryNumber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("c47bf25f-7d16-4dcd-af3b-5e893a1cdd92")]
    #endregion
    public partial class JournalEntryNumber : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("8fd6ce7a-0b08-4af4-9b7f-05a7e12445ed")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public JournalType JournalType { get; set; }

        #region Allors
        [Id("99719445-24e6-445e-8ce1-60c0b5911723")]
        #endregion

        public int Number { get; set; }

        #region Allors
        [Id("a47d5af5-21a8-4d4f-a2be-956ae7da8819")]
        #endregion

        public int Year { get; set; }

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
