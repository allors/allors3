// <copyright file="GeneralLedgerAccountType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("ce5c78ee-f892-4ced-9b21-51d84c77127f")]
    #endregion
    public partial class GeneralLedgerAccountType : Object, ExternalWithPrimaryKey
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string ExternalPrimaryKey { get; set; }
        #endregion

        #region Allors
        [Id("e01a0752-531b-4ee3-a58e-711f377247e1")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

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
