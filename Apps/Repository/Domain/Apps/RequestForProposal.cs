// <copyright file="RequestForProposal.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0112ddd0-14de-43e2-97d3-981766dd957e")]
    #endregion
    [Plural("RequestsForProposal")]
    public partial class RequestForProposal : Request, Versioned
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public RequestState PreviousRequestState { get; set; }

        public RequestState LastRequestState { get; set; }

        public RequestState RequestState { get; set; }

        public InternalOrganisation Recipient { get; set; }

        public string InternalComment { get; set; }

        public string Description { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime RequiredResponseDate { get; set; }

        public RequestItem[] RequestItems { get; set; }

        public string RequestNumber { get; set; }

        public RespondingParty[] RespondingParties { get; set; }

        public Party Originator { get; set; }

        public Currency Currency { get; set; }

        public ContactMechanism FullfillContactMechanism { get; set; }

        public string EmailAddress { get; set; }

        public string TelephoneNumber { get; set; }

        public string TelephoneCountryCode { get; set; }

        public Person ContactPerson { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public int SortableRequestNumber { get; set; }

        #endregion

        #region Allors
        [Id("30472626-909D-4B7E-A153-B2754D6398E3")]
        #endregion
        public void CreateProposal() { }

        #region Versioning
        #region Allors
        [Id("0CDCEB30-489D-4CC7-8C27-B51BCBC3B631")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public RequestForProposalVersion CurrentVersion { get; set; }

        #region Allors
        [Id("2043D481-BB86-4F2E-AE34-506E9F12227F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public RequestForProposalVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods
        public void Cancel() { }

        public void Reject() { }

        public void Submit() { }

        public void Hold() { }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        public void Print() { }

        public void CreateQuote() { }
        #endregion
    }
}
