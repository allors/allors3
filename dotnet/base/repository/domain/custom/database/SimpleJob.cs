// <copyright file="SimpleJob.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("320985b6-d571-4b6c-b940-e02c04ad37d3")]
    #endregion
    public partial class SimpleJob : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("7cd27660-13c6-4a15-8fd8-5775920cfd28")]
        #endregion
        public int Index { get; set; }

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
