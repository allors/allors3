// <copyright file="AgreementSection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("e31d6dd2-b5b2-4fd8-949f-0df688ed2e9b")]
    #endregion
    public partial class AgreementSection : AgreementItem
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
