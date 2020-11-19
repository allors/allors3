// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    public partial class Person : Party, Deletable, Versioned
    {
        #region inherited properties
        public Locale Locale { get; set; }

        public string PartyName { get; set; }

        public ContactMechanism GeneralCorrespondence { get; set; }

        public TelecommunicationsNumber BillingInquiriesFax { get; set; }

        public Qualification[] Qualifications { get; set; }

        public ContactMechanism HomeAddress { get; set; }

        public ContactMechanism SalesOffice { get; set; }

        public PartyContactMechanism[] InactivePartyContactMechanisms { get; set; }

        public TelecommunicationsNumber OrderInquiriesFax { get; set; }

        public PartyContactMechanism[] PartyContactMechanisms { get; set; }

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

        public VatRegime VatRegime { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public PaymentMethod DefaultPaymentMethod { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public PartyRate[] PartyRates { get; set; }

        public bool CollectiveWorkEffortInvoice { get; set; }

        public UserProfile UserProfile { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("F97D0E2B-C361-42BD-AB01-C99E8EDBAB02")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PersonVersion CurrentVersion { get; set; }

        #region Allors
        [Id("B43008FF-7AEB-4599-BB9D-E112E9C80AD9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PersonVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("348dd7c2-c534-422c-90aa-d48b1e504df9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Salutation Salutation { get; set; }

        #region Allors
        [Id("4a01889c-ed4f-41f5-8a25-f0e3bbeb095b")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal YTDCommission { get; set; }

        #region Allors
        [Id("4de34e8b-6c0e-48e7-9b5a-5390325a13ff")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public PersonClassification[] PersonClassifications { get; set; }

        #region Allors
        [Id("539b51e6-dd15-481c-86d3-ceb84588c078")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Indexed]
        public Citizenship Citizenship { get; set; }

        #region Allors
        [Id("5f5d8dd2-33e6-4924-bae7-b6710a789ac9")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal LastYearsCommission { get; set; }

        #region Allors
        [Id("634130cf-b466-4ed3-9036-a4a20566c344")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string GivenName { get; set; }

        #region Allors
        [Id("6d425613-b821-46f2-896a-a04dc4b377a3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public PersonalTitle[] Titles { get; set; }

        #region Allors
        [Id("6f7b0a7f-0b8e-4fbe-b248-b7b90fb18613")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string MothersMaidenName { get; set; }

        #region Allors
        [Id("7bcba7fd-6419-4324-8a11-c56bd46581a1")]
        #endregion
        [Workspace(Default)]
        public DateTime BirthDate { get; set; }

        #region Allors
        [Id("a2ace3b0-e38e-49c8-8c4b-0e97672830c4")]
        #endregion
        [Indexed]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Height { get; set; }

        #region Allors
        [Id("ab9b5c70-3d58-4e2b-a140-f8f1a904da51")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public PersonTraining[] PersonTrainings { get; set; }

        #region Allors
        [Id("b6f28dbd-f20f-44ed-a2e7-476f1a8a5518")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GenderType Gender { get; set; }

        #region Allors
        [Id("d48c94ea-5106-44a2-8eda-959e03480960")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public int Weight { get; set; }

        #region Allors
        [Id("ee6e4476-b1fa-431f-add3-30afe199cdd1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Indexed]
        public Hobby[] Hobbies { get; set; }

        #region Allors
        [Id("eeb16852-431b-4b84-983d-559e64af6dfb")]
        #endregion
        [Workspace(Default)]
        public int TotalYearsWorkExperience { get; set; }

        #region Allors
        [Id("f0708d80-a9cf-47be-9bed-76201fe9f17d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Passport[] Passports { get; set; }

        #region Allors
        [Id("f15d6344-e4f4-4b79-a1af-c6a7417af844")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public MaritalStatus MaritalStatus { get; set; }

        #region Allors
        [Id("f92c5c86-c32a-41e0-99ff-2d94a8d6ccfa")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media Picture { get; set; }

        #region Allors
        [Id("fefb8dc2-cfe5-4078-b3a9-8c4622047c34")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string SocialSecurityNumber { get; set; }

        #region Allors
        [Id("ffda06c0-7dff-42fa-abd5-1ed6fa8c43da")]
        #endregion
        [Workspace(Default)]
        public DateTime DeceasedDate { get; set; }

        #region Allors
        [Id("996809E3-994B-4189-B3A8-77CC1BB99B0A")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Function { get; set; }

        #region Allors
        [Id("38469571-E9E6-473D-BE7B-FA9DB3B67AA0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public OrganisationContactRelationship[] CurrentOrganisationContactRelationships { get; set; }

        #region Allors
        [Id("C6AC83CB-08BF-46FE-8BC0-D777DFD18BB0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public OrganisationContactRelationship[] InactiveOrganisationContactRelationships { get; set; }

        #region Allors
        [Id("042A0F63-EB65-41CD-A32A-A14E99EFE4FB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public ContactMechanism[] CurrentOrganisationContactMechanisms { get; set; }

        #region inherited methods
        public void Delete() { }
        #endregion
    }
}
