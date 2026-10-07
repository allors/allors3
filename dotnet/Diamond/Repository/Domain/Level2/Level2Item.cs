// <copyright file="Level2Item.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // The class of the Level2 domain: a name and its normalized form, which a rule of Level2 derives.
    #region Allors
    [Id("b7e07e31-9d6b-4554-bbe9-d4ac265e1282")]
    #endregion
    public partial class Level2Item : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("7b4f04df-a8b0-43d4-a83f-9fb80de7e519")]
        #endregion
        [Size(256)]
        public string Level2Name { get; set; }

        #region Allors
        [Id("4c472e8d-ee1d-435e-bf8a-e6c1b4624259")]
        #endregion
        [Size(256)]
        [Derived]
        public string Level2NormalizedName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
