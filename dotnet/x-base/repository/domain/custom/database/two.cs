// <copyright file="Two.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("9ec7e136-815c-4726-9991-e95a3ec9e092")]
    #endregion
    public partial class Two : Object, Shared
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("8930c13c-ad5a-4b0e-b3bf-d7cdf6f5b867")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public Shared Shared { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
