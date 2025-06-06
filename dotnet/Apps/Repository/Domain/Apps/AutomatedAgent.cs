// <copyright file="AutomatedAgent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    public partial class AutomatedAgent : Party, Versioned
    {
        #region inherited properties

        public Locale Locale { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public UserProfile UserProfile { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public ContactMechanism GeneralCorrespondence { get; set; }

        public TelecommunicationsNumber BillingInquiriesFax { get; set; }

        public Qualification[] Qualifications { get; set; }

        public ContactMechanism HomeAddress { get; set; }

        public ContactMechanism SalesOffice { get; set; }

        public PartyContactMechanism[] InactivePartyContactMechanisms { get; set; }

        public TelecommunicationsNumber OrderInquiriesFax { get; set; }

        public PartyRelationship[] InactivePartyRelationships { get; set; }

        public Person[] CurrentContacts { get; set; }

        public Person[] InactiveContacts { get; set; }

        public TelecommunicationsNumber ShippingInquiriesFax { get; set; }

        public TelecommunicationsNumber ShippingInquiriesPhone { get; set; }

        public BillingAccount[] BillingAccounts { get; set; }

        public TelecommunicationsNumber OrderInquiriesPhone { get; set; }

        public PartySkill[] PartySkills { get; set; }

        public PartyClassification[] PartyClassifications { get; set; }

        public BankAccount[] BankAccounts { get; set; }

        public ContactMechanism BillingAddress { get; set; }

        public EmailAddress GeneralEmail { get; set; }

        public ShipmentMethod DefaultShipmentMethod { get; set; }

        public Resume[] Resumes { get; set; }

        public ContactMechanism HeadQuarter { get; set; }

        public EmailAddress PersonalEmailAddress { get; set; }

        public TelecommunicationsNumber CellPhoneNumber { get; set; }

        public TelecommunicationsNumber BillingInquiriesPhone { get; set; }

        public ContactMechanism OrderAddress { get; set; }

        public ElectronicAddress InternetAddress { get; set; }

        public Media[] Contents { get; set; }

        public CreditCard[] CreditCards { get; set; }

        public PostalAddress ShippingAddress { get; set; }

        public TelecommunicationsNumber GeneralFaxNumber { get; set; }

        public PartyContactMechanism[] CurrentPartyContactMechanisms { get; set; }

        public PartyRelationship[] CurrentPartyRelationships { get; set; }

        public TelecommunicationsNumber GeneralPhoneNumber { get; set; }

        public Currency PreferredCurrency { get; set; }

        public PaymentMethod DefaultPaymentMethod { get; set; }

        public PartyRate[] PartyRates { get; set; }

        public bool CollectiveWorkEffortInvoice { get; set; }

        public Guid DerivationTrigger { get; set; }

        public string DisplayName { get; set; }

        public string DisplayPhone { get; set; }

        public bool IsUser { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("BBFC2FBA-9E39-4BC7-A7BA-C9D6657AA69D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public AutomatedAgentVersion CurrentVersion { get; set; }

        #region Allors
        [Id("80E56218-00C1-4F81-B130-E9CD28C0B4F0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public AutomatedAgentVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods
        public void ResetPassword() { }

        #endregion
    }
}
