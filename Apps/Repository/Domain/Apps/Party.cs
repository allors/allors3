// <copyright file="Party.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("3bba6e5a-dc2d-4838-b6c4-881f6c8c3013")]
    #endregion
    [Plural("Parties")]
    public partial interface Party : Localised, Auditable, UniquelyIdentifiable, Commentable
    {
        #region Allors
        [Id("BB29E79A-5B37-4CE7-B366-32F67FFD1FA3")]
        #endregion
        [Workspace(Default)]
        string PartyName { get; set; }

        #region Allors
        [Id("130d6e94-51e2-45f9-82d7-380ae7c8aa44")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber BillingInquiriesFax { get; set; }

        #region Allors
        [Id("19c8a5a0-9567-4fc2-bfad-94a549cfa191")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Qualification[] Qualifications { get; set; }

        #region Allors
        [Id("1ad85fce-f2f8-45aa-bf1e-8f5ade34153c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism HomeAddress { get; set; }

        #region Allors
        [Id("1d4e59a6-253f-470e-b9a7-c2c73b67cf2f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism SalesOffice { get; set; }

        #region Allors
        [Id("29da9212-a70f-4ee6-98d7-508687faa2b4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber OrderInquiriesFax { get; set; }

        #region Allors
        [Id("42ab0c4b-52b2-494e-b6a9-cacf55fb002e")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        PartyContactMechanism[] PartyContactMechanisms { get; set; }

        #region Allors
        [Id("25068a0e-15f7-41bd-b16d-a7dd51ca9aa3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PartyContactMechanism[] InactivePartyContactMechanisms { get; set; }

        #region Allors
        [Id("e16b9c8f-cb53-4d58-aa13-ac92d5de1465")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PartyContactMechanism[] CurrentPartyContactMechanisms { get; set; }

        #region Allors
        [Id("09C59A03-B52F-45B3-989A-AC1838B0D13F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        PartyRelationship[] CurrentPartyRelationships { get; set; }

        #region Allors
        [Id("588A09B5-62C6-45CD-8527-2C7BDEB75223")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        PartyRelationship[] InactivePartyRelationships { get; set; }

        #region Allors
        [Id("59500ed1-2de5-45ff-bec7-275c1941d153")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Person[] CurrentContacts { get; set; }

        #region Allors
        [Id("245eaa78-39d9-404f-a4da-ad3718cfc0ca")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Person[] InactiveContacts { get; set; }

        #region Allors
        [Id("436f0ef1-a3ea-439c-9ffd-211c177f5ed1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber ShippingInquiriesFax { get; set; }

        #region Allors
        [Id("4444b0d1-4ade-4fed-88bf-ce9ef275a978")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber ShippingInquiriesPhone { get; set; }

        #region Allors
        [Id("4a46f6aa-d4f9-4e5e-ac17-d77ab0e99c3f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        BillingAccount[] BillingAccounts { get; set; }

        #region Allors
        [Id("4d742fa8-f10b-423e-9341-f8a526838eba")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber OrderInquiriesPhone { get; set; }

        #region Allors
        [Id("4e725bd6-2280-48a2-be89-836b4bd7d002")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        PartySkill[] PartySkills { get; set; }

        #region Allors
        [Id("4e787cf8-9b92-4ab2-8f88-c08bdb90a376")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PartyClassification[] PartyClassifications { get; set; }

        #region Allors
        [Id("52dd7bf8-bb7e-47bd-85b3-f35fba964e5c")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        BankAccount[] BankAccounts { get; set; }

        #region Allors
        [Id("70ada4aa-c51c-4f1d-a3d2-ea6de31cb988")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism BillingAddress { get; set; }

        #region Allors
        [Id("79a5c25a-91e9-4a80-8649-c8abe86e47dd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        ShipmentMethod DefaultShipmentMethod { get; set; }

        #region Allors
        [Id("7dc1e326-76ef-4bac-aae1-d6a26da9d40a")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Resume[] Resumes { get; set; }

        #region Allors
        [Id("89971e75-61e5-4a0c-b7fc-6f4c15866175")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism HeadQuarter { get; set; }

        #region Allors
        [Id("90590830-da80-4afd-ac37-e9fafb59493a")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        EmailAddress PersonalEmailAddress { get; set; }

        #region Allors
        [Id("92c99262-30ed-4265-975b-07140c46af6e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber CellPhoneNumber { get; set; }

        #region Allors
        [Id("95f6db56-0dcf-4d5e-8e81-43e0d72faa85")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber BillingInquiriesPhone { get; set; }

        #region Allors
        [Id("a7720655-a6c1-4f54-a093-b77da985ac5f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism OrderAddress { get; set; }

        #region Allors
        [Id("acf731ab-c856-4553-a2fc-9f88e3ccc258")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] Contents { get; set; }

        #region Allors
        [Id("aecedf16-9e42-4e49-b7ec-e92187262405")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        CreditCard[] CreditCards { get; set; }

        #region Allors
        [Id("c20f82fa-3ba2-4e84-beef-52ba30c92695")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PostalAddress ShippingAddress { get; set; }

        #region Allors
        [Id("ac5a48dc-4115-489a-aa8c-f43268b6bfe3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ElectronicAddress InternetAddress { get; set; }

        #region Allors
        [Id("008618c4-6252-4643-a0a8-e736f9288946")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism GeneralCorrespondence { get; set; }

        #region Allors
        [Id("78cc2859-b815-453f-9bdc-17fe64a853c4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        EmailAddress GeneralEmail { get; set; }

        #region Allors
        [Id("d562d1f0-1f8f-40c5-a346-ae32e498f332")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber GeneralFaxNumber { get; set; }

        #region Allors
        [Id("e2017090-fa3f-420e-a5c5-6a2f5aaacd2f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        TelecommunicationsNumber GeneralPhoneNumber { get; set; }

        #region Allors
        [Id("f0de022f-b94e-4d29-8cdf-99d39ad9add6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Currency PreferredCurrency { get; set; }

        #region Allors
        [Id("fafa35a1-7762-47f7-a9c2-28d3d0623e7c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRegime VatRegime { get; set; }

        #region Allors
        [Id("bf0b654e-405b-46a6-9118-23a6c5746a31")]
        #endregion

        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public IrpfRegime IrpfRegime { get; set; }

        #region Allors
        [Id("29AAF778-37F7-4E29-9EED-16748C376D98")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        PaymentMethod DefaultPaymentMethod { get; set; }

        #region Allors
        [Id("FF4DE48B-DB82-46D0-8544-385425E79BCA")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        PartyRate[] PartyRates { get; set; }

        #region Allors
        [Id("6EC8B6A5-D5E1-4A62-AE55-B12045F059D4")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool CollectiveWorkEffortInvoice { get; set; }
    }
}
