// <copyright file="PersonVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("E42FCDBF-5DEF-4743-BDE8-4028AC6A00A5")]
    #endregion

    public partial class PersonVersion : PartyVersion
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public string Comment { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string PartyName { get; set; }

        public ContactMechanism GeneralCorrespondence { get; set; }

        public TelecommunicationsNumber BillingInquiriesFax { get; set; }

        public Qualification[] Qualifications { get; set; }

        public ContactMechanism HomeAddress { get; set; }

        public ContactMechanism SalesOffice { get; set; }

        public TelecommunicationsNumber OrderInquiriesFax { get; set; }

        public PartyContactMechanism[] PartyContactMechanisms { get; set; }

        public TelecommunicationsNumber ShippingInquiriesFax { get; set; }

        public TelecommunicationsNumber ShippingInquiriesPhone { get; set; }

        public BillingAccount[] BillingAccounts { get; set; }

        public TelecommunicationsNumber OrderInquiriesPhone { get; set; }

        public PartySkill[] PartySkills { get; set; }

        public PartyClassification[] PartyClassifications { get; set; }

        public BankAccount[] BankAccounts { get; set; }

        public ContactMechanism BillingAddress { get; set; }

        public ElectronicAddress GeneralEmail { get; set; }

        public ShipmentMethod DefaultShipmentMethod { get; set; }

        public Resume[] Resumes { get; set; }

        public ContactMechanism HeadQuarter { get; set; }

        public ElectronicAddress PersonalEmailAddress { get; set; }

        public TelecommunicationsNumber CellPhoneNumber { get; set; }

        public TelecommunicationsNumber BillingInquiriesPhone { get; set; }

        public ContactMechanism OrderAddress { get; set; }

        public ElectronicAddress InternetAddress { get; set; }

        public Media[] Contents { get; set; }

        public CreditCard[] CreditCards { get; set; }

        public PostalAddress ShippingAddress { get; set; }

        public TelecommunicationsNumber GeneralFaxNumber { get; set; }

        public TelecommunicationsNumber GeneralPhoneNumber { get; set; }

        public Currency PreferredCurrency { get; set; }

        public VatRegime VatRegime { get; set; }

        public PartyRate[] PartyRates { get; set; }
        #endregion

        #region Allors
        [Id("57345BFB-05EC-429F-88AF-BEE30408B121")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Salutation Salutation { get; set; }

        #region Allors
        [Id("0C856758-192D-41E8-83CA-9471D41AA832")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public PersonClassification[] PersonClassifications { get; set; }

        #region Allors
        [Id("A0025652-7D6B-4AD1-B2A5-93C15D13D427")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public Citizenship Citizenship { get; set; }

        #region Allors
        [Id("1EB80365-57C9-4C63-B7A3-A1A956C098B1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string GivenName { get; set; }

        #region Allors
        [Id("69E4038A-10F6-4623-A68D-550206F7A2FE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public PersonalTitle[] Titles { get; set; }

        #region Allors
        [Id("4E7EEAE8-AA25-49ED-858D-ABB9F63FEF9C")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string MothersMaidenName { get; set; }

        #region Allors
        [Id("DCF909FD-CB0D-4808-8637-2280164F2808")]
        #endregion
        [Workspace(Default)]
        public DateTime BirthDate { get; set; }

        #region Allors
        [Id("7D028617-E3ED-4532-B49B-4B0F04C14EB1")]
        #endregion
        [Indexed]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Height { get; set; }

        #region Allors
        [Id("FF747CE7-24CB-4CA2-A407-DAFBA3CD2AD8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public PersonTraining[] PersonTrainings { get; set; }

        #region Allors
        [Id("C681DA3A-DC8C-4048-B7AE-9B7FF510DA65")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public GenderType Gender { get; set; }

        #region Allors
        [Id("B18564AD-D2A6-46CB-989F-8777E8AE7191")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public int Weight { get; set; }

        #region Allors
        [Id("C85647ED-5173-47CC-A86F-8C3770A53124")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public Hobby[] Hobbies { get; set; }

        #region Allors
        [Id("B9C6121A-FE18-4FD9-BC47-05716061B7A8")]
        #endregion
        [Workspace(Default)]
        public int TotalYearsWorkExperience { get; set; }

        #region Allors
        [Id("E141FDC1-29C9-494F-A6C8-BCA76D0FB364")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public Passport[] Passports { get; set; }

        #region Allors
        [Id("FC3AD9FF-B460-41E7-8433-1B7273D62A3A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public MaritalStatus MaritalStatus { get; set; }

        #region Allors
        [Id("9A2F5050-FA3A-45FF-9363-8A6B2123D35D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public Media Picture { get; set; }

        #region Allors
        [Id("3B0FDE6C-31C0-4633-8083-B01D5EEB7FF1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string SocialSecurityNumber { get; set; }

        #region Allors
        [Id("34104E71-74E3-4BFD-ADE5-CE4BD79A8A0A")]
        #endregion
        [Workspace(Default)]
        public DateTime DeceasedDate { get; set; }

        #region Allors
        [Id("7F28B1AB-7978-444C-9CB6-3E11F739234C")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Function { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
