// <copyright file="RequestForQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("874dfe70-2e50-4861-b26d-dc55bc8fa0d0")]
    #endregion
    [Plural("RequestsForQuote")]
    public partial class RequestForQuote : Request, Versioned
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

        public Currency AssignedCurrency { get; set; }

        public Currency DerivedCurrency { get; set; }

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

        #region Versioning
        #region Allors
        [Id("4C48ECD7-C684-4103-89B2-F5CFC0675124")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public RequestForQuoteVersion CurrentVersion { get; set; }

        #region Allors
        [Id("A14F99B1-A53B-4ABD-B5AC-810FF7CBAC6D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public RequestForQuoteVersion[] AllVersions { get; set; }
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
