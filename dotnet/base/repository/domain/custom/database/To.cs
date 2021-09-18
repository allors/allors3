// <copyright file="To.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("7eb25112-4b81-4e8d-9f75-90950c40c65f")]
    #endregion
    public partial class To : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("4be564ac-77bc-48b8-b945-7d39f2ea9903")]
        [Size(256)]
        #endregion
        public string Name { get; set; }

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
