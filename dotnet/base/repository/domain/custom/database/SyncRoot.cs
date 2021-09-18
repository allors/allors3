// <copyright file="SyncRoot.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("2A863DCF-C6FE-4838-8D3A-1212A2076D70")]
    #endregion
    public partial class SyncRoot : Object, DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        #endregion

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

        #region Allors
        [Id("615C6C58-513A-456F-A0CE-E472D173DCB0")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        #endregion
        [Derived]
        public SyncDepthI1 SyncDepth1 { get; set; }

        #region Allors
        [Id("4061BB19-494D-4CD4-AE7F-798FC62942AB")]
        #endregion
        [Required]
        public int Value { get; set; }
    }
}
