// <copyright file="RequestForInformationVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("CB28933F-4308-43FD-B347-F6773EEC16B5")]
    #endregion
    public partial class RequestForInformationVersion : RequestVersion
    {
        #region inherited properties

        public RequestState RequestState { get; set; }

        public string InternalComment { get; set; }

        public string Description { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime RequiredResponseDate { get; set; }

        public RequestItem[] RequestItems { get; set; }

        public string RequestNumber { get; set; }

        public RespondingParty[] RespondingParties { get; set; }

        public Party Originator { get; set; }

        public Currency AssignedCurrency { get; set; }

        public ContactMechanism FullfillContactMechanism { get; set; }

        public string EmailAddress { get; set; }

        public string TelephoneNumber { get; set; }

        public string TelephoneCountryCode { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public InternalOrganisation Recipient { get; set; }

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
