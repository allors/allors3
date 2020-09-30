// <copyright file="PartyVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("FA4FC65D-8B26-434D-95C9-06991EAA0B57")]
    #endregion
    public partial interface PartyVersion : Version
    {
        #region Allors
        [Id("A2915A2C-0D81-4BE3-8EEA-193692351F52")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Comment { get; set; }

        #region Allors
        [Id("089768C8-5084-4917-8B21-3B185B9FADE6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        User CreatedBy { get; set; }

        #region Allors
        [Id("2BAD0A81-70F0-44DF-8539-280A34206DF6")]
        #endregion
        [Workspace(Default)]
        DateTime CreationDate { get; set; }

        #region Allors
        [Id("5079430B-3610-4906-8F58-B34D6DCD1832")]
        [Workspace(Default)]
        #endregion
        string PartyName { get; set; }

        #region Allors
        [Id("3F2F3296-7DA7-48F7-B172-16904F47FA5F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism GeneralCorrespondence { get; set; }

        #region Allors
        [Id("27A64036-F3FD-4F7A-BF11-5AC3F032C943")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber BillingInquiriesFax { get; set; }

        #region Allors
        [Id("70B38198-DA16-4764-88A6-DF491E376001")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Qualification[] Qualifications { get; set; }

        #region Allors
        [Id("352932DD-2804-43D7-A660-C478A14E35D7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism HomeAddress { get; set; }

        #region Allors
        [Id("5CC1C591-1C3D-4E09-A4CA-05E224C3EC70")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism SalesOffice { get; set; }

        #region Allors
        [Id("33E1CF67-EA30-4DB6-BF95-AA1549D72C0D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber OrderInquiriesFax { get; set; }

        #region Allors
        [Id("6AF7A276-4C4E-4492-A26B-3BFDF38A9BE7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        PartyContactMechanism[] PartyContactMechanisms { get; set; }

        #region Allors
        [Id("599ED873-0645-466F-9156-F14FC42CE14D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber ShippingInquiriesFax { get; set; }

        #region Allors
        [Id("1412C759-965B-45CF-AE03-2699428C082F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber ShippingInquiriesPhone { get; set; }

        #region Allors
        [Id("67AD2282-2742-4335-9CC2-A0ADA1464075")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        BillingAccount[] BillingAccounts { get; set; }

        #region Allors
        [Id("ACCE4AF5-3A14-4C33-AE5D-055A7AE89D6C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber OrderInquiriesPhone { get; set; }

        #region Allors
        [Id("5FBED0ED-AF61-42D0-B78D-E9D19F29B70F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        PartySkill[] PartySkills { get; set; }

        #region Allors
        [Id("7B55B29E-497A-470C-8E91-19782B4E1FA4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        PartyClassification[] PartyClassifications { get; set; }

        #region Allors
        [Id("36BB4124-03C3-429D-AD01-D31BFCF81CCB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        BankAccount[] BankAccounts { get; set; }

        #region Allors
        [Id("25733706-8788-42E5-9EBA-41D2F5179438")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism BillingAddress { get; set; }

        #region Allors
        [Id("51D21BDF-F8C5-4486-851C-C0506C6E6CAF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ElectronicAddress GeneralEmail { get; set; }

        #region Allors
        [Id("C58443ED-7877-4BCA-B8D1-CD109E809220")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        ShipmentMethod DefaultShipmentMethod { get; set; }

        #region Allors
        [Id("35A8E698-2F7A-4DD3-A751-1546D8330929")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        Resume[] Resumes { get; set; }

        #region Allors
        [Id("563009C2-48D8-42E3-99CE-9FE44932E6E4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism HeadQuarter { get; set; }

        #region Allors
        [Id("76B8C934-C3BE-4489-AD19-3F0C75CF61FC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Workspace(Default)]
        ElectronicAddress PersonalEmailAddress { get; set; }

        #region Allors
        [Id("AE543D94-C557-4083-8A9D-3A959D6B70F1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber CellPhoneNumber { get; set; }

        #region Allors
        [Id("3A19126D-388A-499E-9629-DC9C551FB689")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber BillingInquiriesPhone { get; set; }

        #region Allors
        [Id("0B2392E0-CBAC-4916-85D6-9299A740059F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ContactMechanism OrderAddress { get; set; }

        #region Allors
        [Id("F536692F-739B-47AB-9D95-C8B8DEEDBBEC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        ElectronicAddress InternetAddress { get; set; }

        #region Allors
        [Id("2BC6355F-E363-4573-AAE8-4C5FBD835966")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        Media[] Contents { get; set; }

        #region Allors
        [Id("3E92FFDE-FF8E-4793-AEA3-44EBCE373523")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        CreditCard[] CreditCards { get; set; }

        #region Allors
        [Id("FA8FD2C5-0761-4617-999A-3A720D2D643E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        PostalAddress ShippingAddress { get; set; }

        #region Allors
        [Id("0F51981D-446D-4976-B6DB-5F38396C5AD6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber GeneralFaxNumber { get; set; }

        #region Allors
        [Id("B9EBE9E8-C386-443C-A4C6-F15714E23E1B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        TelecommunicationsNumber GeneralPhoneNumber { get; set; }

        #region Allors
        [Id("22CB9D7A-487F-4E8E-9FFC-3A2F3B1AE2C5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Currency PreferredCurrency { get; set; }

        #region Allors
        [Id("36648D37-BBD7-4C78-87D0-8CB77DABF7ED")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        VatRegime VatRegime { get; set; }

        #region Allors
        [Id("E25BB549-9B1B-4FAF-A62C-72EB95897D0F")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        PartyRate[] PartyRates { get; set; }
    }
}
