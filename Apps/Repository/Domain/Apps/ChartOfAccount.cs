// <copyright file="ChartOfAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("6cf4845d-65a0-4957-95e9-f2b5327d6515")]
    #endregion
    [Plural("ChartsOfAccounts")]
    public partial class ChartOfAccounts : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("65f44f44-a613-4cbf-a924-1098c9876f20")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

        #region Allors
        [Id("71d503fb-ebb9-45b3-af62-1b233677adce")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public GeneralLedgerAccount[] GeneralLedgerAccounts { get; set; }

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
