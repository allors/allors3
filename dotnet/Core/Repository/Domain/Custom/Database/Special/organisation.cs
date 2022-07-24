// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3a5dcec7-308f-48c7-afee-35d38415aa0b")]
    #endregion
    [Workspace(Default)]
    public partial class Organisation : Deletable, UniquelyIdentifiable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("2cfea5d4-e893-4264-a966-a68716839acd")]
        [Size(-1)]
        #endregion
        public string Description { get; set; }

        #region Allors
        [Id("49b96f79-c33d-4847-8c64-d50a6adb4985")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public Person[] Employees { get; set; }

        #region Allors
        [Id("dbef262d-7184-4b98-8f1f-cf04e884bb92")]
        [Indexed]
        [Workspace(Default)]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public Person Manager { get; set; }

        #region Allors
        [Id("845ff004-516f-4ad5-9870-3d0e966a9f7d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person Owner { get; set; }

        #region Allors
        [Id("15f33fa4-c878-45a0-b40c-c5214bce350b")]
        [Indexed]
        [Workspace(Default)]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public Person[] Shareholders { get; set; }

        #region Allors
        [Id("5fa25b53-e2a7-44c8-b6ff-f9575abb911d")]
        #endregion
        public bool Incorporated { get; set; }

        #region Allors
        [Id("7046c2b4-d458-4343-8446-d23d9c837c84")]
        #endregion
        public DateTime IncorporationDate { get; set; }

        #region Allors
        [Id("01dd273f-cbca-4ee7-8c2d-827808aba481")]
        [Indexed]
        [Size(-1)]
        #endregion
        public string Information { get; set; }

        #region Allors
        [Id("68c61cea-4e6e-4ed5-819b-7ec794a10870")]
        #endregion
        public bool IsSupplier { get; set; }

        #region Allors
        [Id("2cc74901-cda5-4185-bcd8-d51c745a8437")]
        [Indexed]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("bac702b8-7874-45c3-a410-102e1caea4a7")]
        [Size(256)]
        #endregion
        public string Size { get; set; }

        #region Allors
        [Id("D3DB6E8C-9C10-47BA-92B1-45F5DDFFA5CC")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Person CycleOne { get; set; }

        #region Allors
        [Id("C6CCA1C5-5799-4517-87F5-095DA0EEEC64")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public Person[] CycleMany { get; set; }

        #region Allors
        [Id("607C1D85-E722-40BC-A4D6-0C6A7244AF6A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Data OneData { get; set; }

        #region Allors
        [Id("897DA15E-C250-441F-8F5C-6F9F3E7870EB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public Data[] ManyDatas { get; set; }

        #region Allors
        [Id("BC94072C-8F69-45AE-AED0-C056758F78F3")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool JustDidIt { get; set; }

        #region Allors
        [Id("BABDCD10-5A5C-417A-9D34-2EA47F207A91")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public bool JustDidItDerived { get; set; }

        #region Allors
        [Id("3F73377A-C2F5-4560-B493-B6F23B69F2DA")]
        #endregion
        [Required]
        public bool PostDeriveTrigger { get; set; }

        #region Allors
        [Id("A05EAA74-39A5-4613-B727-EAEA48416299")]
        #endregion
        [Required]
        public bool PostDeriveTriggered { get; set; }

        #region Allors
        [Id("030DD8E2-0953-462F-95D9-8B7706AAE1C4")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string DisplayName { get; set; }

        [Id("1869873F-F2F0-4D03-A0F9-7DC73491C117")]
        [Workspace(Default)]
        public void JustDoIt() { }

        [Id("2CD2FF48-93FC-4C7D-BF2F-3F411D0DF7C3")]
        [Workspace(Default)]
        public void ToggleCanWrite() { }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
