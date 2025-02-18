// <copyright file="VatReturnBox.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("8dc67774-c15a-47dd-9b8a-ce4e7139e8a3")]
    #endregion
    public partial class VatBox : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("3bcc4fc9-5646-4ceb-b48b-bb1d7fbcba64")]
        #endregion
        [Size(256)]
        public string BoxNumber { get; set; }

        #region Allors
        [Id("78e114b4-ec1d-49ce-ab32-40a3184dea31")]
        #endregion
        public string Name { get; set; }

        #region Allors
        [Id("beb55438-918d-4876-8d0b-989b7d9fabfa")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public VatBoxType VatBoxType { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
