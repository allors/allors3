// <copyright file="BadUI.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("bb1b0a2e-66d1-4e09-860f-52dc7145029e")]
    #endregion
    public partial class BadUI : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("8a999086-ca90-40a1-90ae-475d231bb1eb")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        #endregion
        public Person[] PersonsMany { get; set; }

        #region Allors
        [Id("9ebbb9d1-2ca7-4a7f-9f18-f25c05fd28c1")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public Organisation CompanyOne { get; set; }

        #region Allors
        [Id("a4db0d75-3dff-45ac-9c1d-623bca046b4a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public Person PersonOne { get; set; }

        #region Allors
        [Id("a8621048-48b5-43c4-b10b-17225958d177")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public Organisation CompanyMany { get; set; }

        #region Allors
        [Id("c93a102e-ecdb-4189-a0fc-eeea8b4b85d4")]
        [Size(256)]
        #endregion
        public string AllorsString { get; set; }

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
