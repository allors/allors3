// <copyright file="LegalTerm.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("14a2576c-3ea7-4016-aba2-44172fb7a952")]
    #endregion
    public partial class LegalTerm : AgreementTerm
    {
        #region inherited properties
        public string TermValue { get; set; }

        public TermType TermType { get; set; }

        public string Description { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion

    }
}
