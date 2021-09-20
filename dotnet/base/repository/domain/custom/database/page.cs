// <copyright file="Place.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0777C78C-CB50-4FDD-8386-5BCEC00B208C")]
    #endregion
    public partial class Page : UniquelyIdentifiable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("9B2F32B4-DF88-41DA-AE4C-A7A8D4232C1C")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("E3117BBB-3B1E-465A-8DD0-CC5FE3A5A905")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public Media Content { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
