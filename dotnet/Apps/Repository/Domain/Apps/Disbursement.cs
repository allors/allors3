// <copyright file="Disbursement.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("d152e0a4-c76f-4945-8c0f-ad1e5f70ad07")]
    #endregion
    public partial class Disbursement : Payment
    {
        #region inherited properties

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime EffectiveDate { get; set; }

        public Party Sender { get; set; }

        public PaymentApplication[] PaymentApplications { get; set; }

        public string ReferenceNumber { get; set; }

        public Party Receiver { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Guid UniqueId { get; set; }

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
