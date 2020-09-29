// <copyright file="VatReturnBox.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("8dc67774-c15a-47dd-9b8a-ce4e7139e8a3")]
    #endregion
    public partial class VatReturnBox : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("3bcc4fc9-5646-4ceb-b48b-bb1d7fbcba64")]
        #endregion
        [Size(256)]

        public string HeaderNumber { get; set; }

        #region Allors
        [Id("78e114b4-ec1d-49ce-ab32-40a3184dea31")]
        #endregion
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("beb55438-918d-4876-8d0b-989b7d9fabfa")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public VatReturnBoxType VatReturnBoxType { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}
