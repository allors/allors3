// <copyright file="OrganisationVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("E1AFA103-7032-416B-AC7B-274A7E35381A")]
    #endregion
    public partial class OrganisationVersion : PartyVersion
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
        [Id("45CDF666-F891-4F5D-8583-0CB5D489C918")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        public SecurityToken ContactsSecurityToken { get; set; }

        #region Allors
        [Id("7BE2E3DF-E4B8-4D4F-BA1C-89E8E123D79B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        public AccessControl ContactsAccessControl { get; set; }

        #region Allors
        [Id("3A40A81A-049D-40E9-93AA-6DDA5CDCC450")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        public UserGroup OwnerUserGroup { get; set; }

        #region Allors
        [Id("FB2498BD-0CC8-4D3A-BA00-1F30B71A856B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public LegalForm LegalForm { get; set; }

        #region Allors
        [Id("B5552663-3D07-4EEA-BA8A-DEB40D264D48")]
        #endregion
        [Indexed]
        [Size(256)]
        [Workspace]
        public string Name { get; set; }

        #region Allors
        [Id("F0038F19-2997-4A5D-AF28-6019638E9264")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        public UserGroup ContactsUserGroup { get; set; }

        #region Allors
        [Id("A6D98EB2-0911-436E-81FD-0ABC723346B4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public Media LogoImage { get; set; }

        #region Allors
        [Id("5910ADAC-B422-4599-9F6D-38489BA7023B")]
        #endregion
        [Size(256)]
        [Workspace]
        public string TaxNumber { get; set; }

        #region Allors
        [Id("9D37EDBB-ED85-4966-8B14-D26BC8A8BF52")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public IndustryClassification[] IndustryClassifications { get; set; }

        #region Allors
        [Id("E6C1CB71-592C-4BFD-8B26-4ABD4EB03A72")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public CustomOrganisationClassification[] CustomClassifications { get; set; }

        #region Allors
        [Id("debd810e-6678-42db-9f2a-e4b871039627")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public IrpfRegime IrpfRegime { get; set; }

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
