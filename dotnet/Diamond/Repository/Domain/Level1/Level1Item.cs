// <copyright file="Level1Item.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // The class of the Level1 domain: a name and its normalized form, which a rule of Level1
    // derives. A domain that extends Level1 may add roles to this class in a partial of its own.
    #region Allors
    [Id("75e4b867-f71f-479f-ae9b-d86ab132750a")]
    #endregion
    public partial class Level1Item : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("677718a6-e81e-495c-9e4c-9c2f5f285f1a")]
        #endregion
        [Size(256)]
        public string Level1Name { get; set; }

        #region Allors
        [Id("c9826574-d99d-4dc0-b2b6-db17f46e0aa4")]
        #endregion
        [Size(256)]
        [Derived]
        public string Level1NormalizedName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
