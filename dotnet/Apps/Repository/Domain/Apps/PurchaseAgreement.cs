// <copyright file="PurchaseAgreement.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("1032dc2f-72b7-4ba2-b47d-ba14d52a18c9")]
    #endregion
    public partial class PurchaseAgreement : Agreement
    {
        #region inherited properties
        public DateTime AgreementDate { get; set; }

        public Addendum[] Addenda { get; set; }

        public string Description { get; set; }

        public AgreementTerm[] AgreementTerms { get; set; }

        public string Text { get; set; }

        public AgreementItem[] AgreementItems { get; set; }

        public string AgreementNumber { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

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
