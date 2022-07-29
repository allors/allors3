// <copyright file="HomeAddress.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2561e93c-5b85-44fb-a924-a1c0d1f78846")]
    #endregion
    [Plural("HomeAddresses")]
    [Workspace(Default)]
    public partial class HomeAddress : Object, Address
    {
        #region inherited properties
        public Place Place { get; set; }

        #endregion

        #region Allors
        [Id("6f0f42c4-9b47-47c2-a632-da8e08116be4")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        public string Street { get; set; }

        #region Allors
        [Id("b181d077-e897-4add-9456-67b9760d32e8")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        public string HouseNumber { get; set; }

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
