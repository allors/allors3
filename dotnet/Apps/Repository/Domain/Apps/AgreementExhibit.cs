// <copyright file="AgreementExhibit.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("2830c388-b002-44d6-91b6-b2b43fa778f3")]
    #endregion
    public partial class AgreementExhibit : AgreementItem
    {
        #region inherited properties
        public string Text { get; set; }

        public Addendum[] Addenda { get; set; }

        public AgreementItem[] Children { get; set; }

        public string Description { get; set; }

        public AgreementTerm[] AgreementTerms { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }

        #endregion

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
