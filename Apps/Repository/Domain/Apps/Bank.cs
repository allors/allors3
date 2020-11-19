// <copyright file="Bank.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors

    [Id("a24a8e12-7067-4bfb-8fc0-225a824d1a05")]

    #endregion

    public partial class Bank : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("28723704-3a61-445a-b14e-c757ebbf8d66")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Media Logo { get; set; }

        #region Allors
        [Id("354e114f-5d6b-4883-8e58-5c7a39878b6d")]
        #endregion
        [Required]
        [Size(256)]
        public string Bic { get; set; }

        #region Allors
        [Id("a7851af8-38cd-4785-b81c-fb2fa403d9f6")]
        #endregion
        [Size(256)]
        public string SwiftCode { get; set; }

        #region Allors
        [Id("d3a11d21-0232-48a0-b784-c111ad05f5da")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Country Country { get; set; }

        #region Allors
        [Id("d4191223-d9be-4cbb-b2ad-ee0844dcae87")]
        #endregion
        [Required]
        [Size(256)]
        public string Name { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPreDerive()
        {
        }

        public void OnDerive()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion
    }
}
