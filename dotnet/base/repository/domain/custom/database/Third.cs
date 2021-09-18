// <copyright file="Third.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("39116edf-34cf-45a6-ac09-2e4f98f28e14")]
    #endregion
    public partial class Third : Object, DerivationCounted
    {
        #region inherited properties

        public int DerivationCount { get; set; }

        #endregion

        #region Allors
        [Id("6ab5a7af-a0f0-4940-9be3-6f6430a9e728")]
        #endregion
        public bool IsDerived { get; set; }

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
