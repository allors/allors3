// <copyright file="Transition.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("a7e490c0-ce29-4298-97c4-519904bb755a")]
    #endregion
    public partial class Transition : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("c6ee1a42-05fa-462b-b04f-811f01c6b646")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]

        public ObjectState[] FromStates { get; set; }

        #region Allors
        [Id("dd19e7f8-83b7-4ff1-b475-02c4296b47e4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public ObjectState ToState { get; set; }

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
