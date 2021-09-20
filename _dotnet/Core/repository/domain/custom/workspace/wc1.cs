// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("88d3a45b-2094-437d-8338-6f82caeeefb8")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Workspace)]
    public partial class WC1 : WI1, DerivationCounted, Object
    {
        #region inherited properties
        public WI1 WI1WI1Many2One { get; set; }

        public WI12[] WI1WI12Many2Manies { get; set; }

        public WI2[] WI1WI2Many2Manies { get; set; }

        public WI2 WI1WI2Many2One { get; set; }

        public string WI1AllorsString { get; set; }

        public WI12 WI1WI12Many2One { get; set; }

        public DateTime WI1AllorsDateTime { get; set; }

        public WI2[] WI1WI2One2Manies { get; set; }

        public WC2[] WI1WC2One2Manies { get; set; }

        public WC1 WI1WC1One2One { get; set; }

        public int WI1AllorsInteger { get; set; }

        public WC2[] WI1WC2Many2Manies { get; set; }

        public WI1[] WI1WI1One2Manies { get; set; }

        public WI1[] WI1WI1Many2Manies { get; set; }

        public bool WI1AllorsBoolean { get; set; }

        public decimal WI1AllorsDecimal { get; set; }

        public WI12 WI1WI12One2One { get; set; }

        public WI2 WI1WI2One2One { get; set; }

        public WC2 WI1WC2One2One { get; set; }

        public WC1[] WI1WC1One2Manies { get; set; }

        public byte[] WI1AllorsBinary { get; set; }

        public WC1[] WI1WC1Many2Manies { get; set; }

        public double WI1AllorsDouble { get; set; }

        public WI1 WI1WI1One2One { get; set; }

        public WC1 WI1WC1Many2One { get; set; }

        public WI12[] WI1WI12One2Manies { get; set; }

        public WC2 WI1WC2Many2One { get; set; }

        public Guid WI1AllorsUnique { get; set; }

        public byte[] WI12AllorsBinary { get; set; }

        public WC2 WI12WC2One2One { get; set; }

        public double WI12AllorsDouble { get; set; }

        public WI1 WI12WI1Many2One { get; set; }

        public string WI12AllorsString { get; set; }

        public WI12[] WI12WI12Many2Manies { get; set; }

        public decimal WI12AllorsDecimal { get; set; }

        public WI2[] WI12WI2Many2Manies { get; set; }

        public WC2[] WI12WC2Many2Manies { get; set; }

        public WI1[] WI12WI1Many2Manies { get; set; }

        public WI12[] WI12WI12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public WC1[] WI12WC1Many2Manies { get; set; }

        public WI2 WI12WI2Many2One { get; set; }

        public Guid WI12AllorsUnique { get; set; }

        public int WI12AllorsInteger { get; set; }

        public WI1[] WI12WI1One2Manies { get; set; }

        public WC1 WI12WC1One2One { get; set; }

        public WI12 WI12WI12One2One { get; set; }

        public WI2 WI12WI2One2One { get; set; }

        public WI12[] WorkspaceDependencies { get; set; }

        public WI2[] WI12WI2One2Manies { get; set; }

        public WC2 WI12WC2Many2One { get; set; }

        public WI12 WI12WI12Many2One { get; set; }

        public bool WI12AllorsBoolean { get; set; }

        public WI1 WI12WI1One2One { get; set; }

        public WC1[] WI12WC1One2Manies { get; set; }

        public WC1 WI12WC1Many2One { get; set; }

        public DateTime WI12AllorsDateTime { get; set; }

        public I1 WI1DatabaseI1Many2One { get; set; }

        public I12[] WI1DatabaseI12Many2Manies { get; set; }

        public I2[] WI1DatabaseI2Many2Manies { get; set; }

        public I2 WI1DatabaseI2Many2One { get; set; }

        public string DatabaseI1AllorsString { get; set; }

        public I12 WI1DatabaseI12Many2One { get; set; }

        public DateTime DatabaseI1AllorsDateTime { get; set; }

        public I2[] WI1DatabaseI2One2Manies { get; set; }

        public C2[] WI1DatabaseC2One2Manies { get; set; }

        public C1 WI1DatabaseC1One2One { get; set; }

        public int DatabaseI1AllorsInteger { get; set; }

        public C2[] WI1DatabaseC2Many2Manies { get; set; }

        public I1[] WI1DatabaseI1One2Manies { get; set; }

        public I1[] WI1DatabaseI1Many2Manies { get; set; }

        public bool DatabaseI1AllorsBoolean { get; set; }

        public decimal DatabaseI1AllorsDecimal { get; set; }

        public I12 WI1DatabaseI12One2One { get; set; }

        public I2 WI1DatabaseI2One2One { get; set; }

        public C2 WI1DatabaseC2One2One { get; set; }

        public C1[] WI1DatabaseC1One2Manies { get; set; }

        public byte[] DatabaseI1AllorsBinary { get; set; }

        public C1[] WI1DatabaseC1Many2Manies { get; set; }

        public double DatabaseI1AllorsDouble { get; set; }

        public I1 WI1DatabaseI1One2One { get; set; }

        public C1 WI1DatabaseC1Many2One { get; set; }

        public I12[] WI1DatabaseI12One2Manies { get; set; }

        public C2 WI1DatabaseC2Many2One { get; set; }

        public Guid DatabaseI1AllorsUnique { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 WI12DatabaseC2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 WI12DatabaseI1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] WI12DatabaseI12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] WI12DatabaseI2Many2Manies { get; set; }

        public C2[] WI12DatabaseC2Many2Manies { get; set; }

        public I1[] WI12DatabaseI1Many2Manies { get; set; }

        public I12[] WI12DatabaseI12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] WI12DatabaseC1Many2Manies { get; set; }

        public I2 WI12DatabaseI2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] WI12DatabaseI1One2Manies { get; set; }

        public C1 WI12DatabaseC1One2One { get; set; }

        public I12 WI12DatabaseI12One2One { get; set; }

        public I2 WI12DatabaseI2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] WI12DatabaseI2One2Manies { get; set; }

        public C2 WI12DatabaseC2Many2One { get; set; }

        public I12 WI12DatabaseI12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 WI12DatabaseI1One2One { get; set; }

        public C1[] WI12DatabaseC1One2Manies { get; set; }

        public C1 WI12DatabaseC1Many2One { get; set; }

        public DateTime DatabaseI12AllorsDateTime { get; set; }


        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public int DerivationCount { get; set; }

        public bool ChangedRolePingS12 { get; set; }
        public bool ChangedRolePongS12 { get; set; }
        public bool ChangedRolePingI12 { get; set; }
        public bool ChangedRolePongI12 { get; set; }
        public bool ChangedRolePingI1 { get; set; }
        public bool ChangedRolePongI1 { get; set; }
        public bool ChangedRolePingC1 { get; set; }
        public bool ChangedRolePongC1 { get; set; }


        #endregion

        #region Workspace Relation
        #region Unit Role
        #region Allors
        [Id("b1168c3c-6379-4824-86c4-1f5ad22fd53c")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public byte[] WorkspaceAllorsBinary { get; set; }

        #region Allors
        [Id("5d2437da-20f5-48a4-938a-c00d3a6e479c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public bool WorkspaceAllorsBoolean { get; set; }

        #region Allors
        [Id("0e99f73f-3360-4aae-865b-f4ee6d58b136")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceAllorsDateTime { get; set; }

        #region Allors
        [Id("b2b24f60-f865-4e9c-8a62-d35a5ac86fb0")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeLessThan { get; set; }

        #region Allors
        [Id("89939dc5-99d6-4bb5-a1d9-260c026f02df")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeGreaterThan { get; set; }

        #region Allors
        [Id("23b405e3-f1a0-4898-a7e8-75a5f1e92285")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeBetweenA { get; set; }

        #region Allors
        [Id("9a0e8019-54bf-4521-ae53-76ca43e00338")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeBetweenB { get; set; }

        #region Allors
        [Id("37dae9c1-6a1b-4d6f-bd25-48a3b2bb8d74")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public decimal WorkspaceAllorsDecimal { get; set; }

        #region Allors
        [Id("9dfb0777-51bf-4906-92a8-bad22dc12c2f")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalLessThan { get; set; }

        #region Allors
        [Id("5154f91c-7c7d-433d-90f1-1f7d07f74d1d")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalGreaterThan { get; set; }

        #region Allors
        [Id("500e4d60-ca74-4ed2-8e62-198a44899236")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalBetweenA { get; set; }

        #region Allors
        [Id("ad3ce131-e19c-4902-9c77-f3a2abc7eb43")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalBetweenB { get; set; }

        #region Allors
        [Id("6b5971b4-4d9f-4a9d-b2c0-6e5f2f372333")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public double WorkspaceAllorsDouble { get; set; }

        #region Allors
        [Id("c6d5fa47-913d-41dc-96b2-e7f16d0453a9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleLessThan { get; set; }

        #region Allors
        [Id("eed21466-9bd4-4faf-8729-e90af9eff00a")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleGreaterThan { get; set; }

        #region Allors
        [Id("0eba124c-93cb-481d-a542-76c740ab30cc")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleBetweenA { get; set; }

        #region Allors
        [Id("8a3e4e87-14db-4448-be59-8908d76fbc91")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleBetweenB { get; set; }

        #region Allors
        [Id("b4666eec-97bd-4f39-aafc-8fd6ee485c80")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceAllorsInteger { get; set; }

        #region Allors
        [Id("8482d1d2-4787-484e-968d-429f58156cc1")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerLessThan { get; set; }

        #region Allors
        [Id("aebfcffb-b5bc-48a8-853a-01e5433b353d")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerGreaterThan { get; set; }

        #region Allors
        [Id("9ff46560-bc8b-4c7f-afc0-2cf4968bbfa5")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerBetweenA { get; set; }

        #region Allors
        [Id("455a02c0-aaff-4c62-8d7a-551e63a1c117")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerBetweenB { get; set; }

        #region Allors
        [Id("b2b5703f-7164-4ea3-8d31-6f13c1836942")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsString { get; set; }

        #region Allors
        [Id("d9fa46d0-1868-4f4c-bffa-87afeb82b21a")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsStringEquals { get; set; }

        #region Allors
        [Id("f383827f-aaba-4aef-ae6f-56ae65e27a0a")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsStringMax { get; set; }

        #region Allors
        [Id("9c20a934-9542-427e-9e80-3f33e8c3a66c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Guid WorkspaceAllorsUnique { get; set; }
        #endregion

        #region Workspace Role
        #region Allors
        [Id("59c35948-3a12-46f2-b378-251c36f60d40")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1[] WorkspaceWC1Many2Manies { get; set; }

        #region Allors
        [Id("13a1ac52-cbd3-4219-b3e1-7c7fdcdf90e2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1 WorkspaceWC1Many2One { get; set; }

        #region Allors
        [Id("fb60c9ae-0a9e-4fa2-8fa2-d5a6960bcfe1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1[] WorkspaceWC1One2Manies { get; set; }

        #region Allors
        [Id("d4404cb1-c9b9-4ebe-9568-8dddc0a9e482")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1 WorkspaceWC1One2One { get; set; }

        #region Allors
        [Id("c7a013df-c73a-4878-911b-7c17eae52bfb")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2[] WorkspaceWC2Many2Manies { get; set; }

        #region Allors
        [Id("af8fdd90-5288-47bd-9df8-83a410836bde")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2 WorkspaceWC2Many2One { get; set; }

        #region Allors
        [Id("bab4db1f-8c83-4c78-b268-6b79c1649266")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2[] WorkspaceWC2One2Manies { get; set; }

        #region Allors
        [Id("f549984d-d894-4bd4-a8b3-b0facfb261d3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2 WorkspaceWC2One2One { get; set; }

        #region Allors
        [Id("9f4e49b9-2819-4517-a6a5-38f9c08c5288")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12[] WorkspaceWI12Many2Manies { get; set; }

        #region Allors
        [Id("21133757-db64-4096-b939-d52ac6a713ae")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12 WorkspaceWI12Many2One { get; set; }

        #region Allors
        [Id("649d7d79-5a85-4d2a-8604-92df80d1f532")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12[] WorkspaceWI12One2Manies { get; set; }

        #region Allors
        [Id("053edc8b-a580-4c57-b040-7345ac8f4d95")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12 WorkspaceWI12One2One { get; set; }

        #region Allors
        [Id("b3dbe29e-8782-45f7-b3c3-5a4a3c9b127f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1[] WorkspaceWI1Many2Manies { get; set; }

        #region Allors
        [Id("0df01347-03d5-467b-a7a7-fb4114964f6c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1 WorkspaceWI1Many2One { get; set; }

        #region Allors
        [Id("322c8a86-ea0a-4f09-844b-94f714e2e7f3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1[] WorkspaceWI1One2Manies { get; set; }

        #region Allors
        [Id("ccb4599d-512d-4868-b0d2-d4c49e9101ac")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1 WorkspaceWI1One2One { get; set; }

        #region Allors
        [Id("d62c480e-4803-47ec-94cf-f19c21357f22")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2[] WorkspaceWI2Many2Manies { get; set; }

        #region Allors
        [Id("635ef67a-65ca-4c5b-b8d3-cc0009317cd5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2 WorkspaceWI2Many2One { get; set; }

        #region Allors
        [Id("f7274131-b157-4e6e-bd20-20273e098cbd")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] WorkspaceWI2One2Manies { get; set; }

        #region Allors
        [Id("a501851a-b485-440b-a371-ac1699f97821")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2 WorkspaceWI2One2One { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("09f2203a-e609-480f-899e-7233004205c1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("1e3f7088-55d5-4e07-91c9-96e08c06d826")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("05e5b19a-ac7f-49dd-be46-ab5a27f83b8e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("b989909b-22b0-4355-a3c8-f769beb0528c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("45491d2d-ba05-43ad-ac8b-c9c4468556af")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("a583ad5e-d6a5-43de-b8f7-c2a0de0d629e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WWorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("3764a1a2-94bc-45bf-be19-030956dbd819")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("321b9054-b325-4be0-890a-5af5696f68b6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("21278eee-e78a-494d-b062-a16cba401fd8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("64da3515-fa2a-4121-b4a7-151e85da423b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("d0d85fbe-5815-4cdd-b6a9-b519a5ed58b5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("09cb1c23-1b2c-47e0-806c-f5739b8f74a9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("5253065d-9253-41fa-afc8-b6b9f4bfe325")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("b57a38db-69d2-4837-9e3a-afb85dbbe6ab")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("65670bd0-f3a2-46d7-902c-08d5380c7fa2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("1800ec28-b5dc-43a5-bece-919b6ca24da0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("86208458-2000-40a7-aba9-2d60f1310413")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2[] WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("45929620-ce95-4dfb-b4ed-005266e40a5e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("0b39c9cd-923e-4024-aa93-6e02d71bf567")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("3567d86a-e3cb-4bbe-aef1-0b30fa18161e")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 WWorkspaceI2One2One { get; set; }
        #endregion
        #endregion

        #region Session Relation
        #region Unit Role
        #region Allors
        [Id("4BE65F01-6536-4D56-8A14-E05B7F893752")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SessionAllorsBinary { get; set; }

        #region Allors
        [Id("F5E13C21-DB5B-4755-8136-D42520802F17")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SessionAllorsBoolean { get; set; }

        #region Allors
        [Id("2DFEF023-98B8-4422-B529-1AFDDEA3B2E4")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionAllorsDateTime { get; set; }

        #region Allors
        [Id("01FD92BB-B253-473F-92C9-22F1CE48561A")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeLessThan { get; set; }

        #region Allors
        [Id("1912F005-7587-4A71-ACDD-F939283FE55B")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeGreaterThan { get; set; }

        #region Allors
        [Id("2C7DB822-DA3F-41BB-811E-509672AAFB9B")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenA { get; set; }

        #region Allors
        [Id("35E2DBCB-6938-4ACF-A6DD-99FD7A4E93EF")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenB { get; set; }

        #region Allors
        [Id("36EE7637-EBC1-44A0-A096-5FD0D1BD6DE9")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SessionAllorsDecimal { get; set; }

        #region Allors
        [Id("12F3D2A1-C021-4718-A82D-1B847722C197")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalLessThan { get; set; }

        #region Allors
        [Id("C8BF4511-42FB-48C6-BFC2-46E216779AC7")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalGreaterThan { get; set; }

        #region Allors
        [Id("BAF21170-CCCA-4B15-AF60-71ED8525179C")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenA { get; set; }

        #region Allors
        [Id("3CF434C5-24AB-4EDF-8E14-63F83CBC48AC")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenB { get; set; }

        #region Allors
        [Id("15108C3F-765B-4682-8BF7-C7E62847157E")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SessionAllorsDouble { get; set; }

        #region Allors
        [Id("DB51454A-C029-4041-9DB0-59460949A4C6")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleLessThan { get; set; }

        #region Allors
        [Id("23FA069C-139E-4448-835B-D90984461497")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleGreaterThan { get; set; }

        #region Allors
        [Id("D25FB2EE-59E0-4DA6-AE1A-4A0A921BF5B7")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenA { get; set; }

        #region Allors
        [Id("F96CCC49-A91D-4AF3-B09F-12A233EFDD56")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenB { get; set; }

        #region Allors
        [Id("1CB07FD2-87CB-42B1-AB7B-380C3D091A95")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionAllorsInteger { get; set; }

        #region Allors
        [Id("F16A2DD2-DF22-4FA5-B85F-CC16FFEDB829")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerLessThan { get; set; }

        #region Allors
        [Id("37EE94B8-AF81-4ECE-B0F6-109C3714B92B")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerGreaterThan { get; set; }

        #region Allors
        [Id("F0CBD8D4-78C0-4703-938A-4431C0D35515")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenA { get; set; }

        #region Allors
        [Id("337EDA2E-A7BB-4B1B-8DD3-1756175C66D1")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenB { get; set; }

        #region Allors
        [Id("9E2BD710-5380-44E1-8DF8-E0F4C5F8E57F")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsString { get; set; }

        #region Allors
        [Id("9CBEB7A8-1714-414B-9193-59F9E68A1B1D")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringEquals { get; set; }

        #region Allors
        [Id("693D93E7-90D5-4E81-85C9-57458FF29C55")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringMax { get; set; }

        #region Allors
        [Id("69D0B09A-B9FB-4665-BF2B-94A75CCA58AA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid SessionAllorsUnique { get; set; }
        #endregion

        #region Session Role
        #region Allors
        [Id("3F599D1C-B6BE-4D78-B1D1-A589E217899E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1Many2Manies { get; set; }

        #region Allors
        [Id("3E14E862-8E02-44FF-A141-2B98E6FFFA1A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1Many2One { get; set; }

        #region Allors
        [Id("7D29953D-67D0-403C-B932-6336C4F76D19")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1One2Manies { get; set; }

        #region Allors
        [Id("099A143A-B5D1-491A-866E-C266D9F5D5D2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1One2One { get; set; }

        #region Allors
        [Id("4B64EDF9-C70D-44E7-BAF3-ABE7A3BED7FB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2Many2Manies { get; set; }

        #region Allors
        [Id("22CE4144-2F78-4A0C-8577-583DE32ED29C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2Many2One { get; set; }

        #region Allors
        [Id("46CC02FC-2E70-426F-838B-A6157ADFE105")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2One2Manies { get; set; }

        #region Allors
        [Id("C4F8FE73-2173-46E5-96F7-9D10F563B174")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2One2One { get; set; }

        #region Allors
        [Id("B9C1673E-26D8-44EF-AE51-C2D10F65D6B2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12Many2Manies { get; set; }

        #region Allors
        [Id("FE4B6597-54D0-4168-AF68-CFADBEDD1E3F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12Many2One { get; set; }

        #region Allors
        [Id("A8D817D3-4C44-4822-98B0-9CDD996FF82D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12One2Manies { get; set; }

        #region Allors
        [Id("63BE4C3F-C1E7-4E21-9F9E-F50B4919AF74")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12One2One { get; set; }

        #region Allors
        [Id("3F4DA12B-F9FB-47BF-B2B9-FC7CE90CFA44")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1Many2Manies { get; set; }

        #region Allors
        [Id("FD047128-A780-4697-9B33-F407EDA1B7D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1Many2One { get; set; }

        #region Allors
        [Id("73B0BAFB-116A-4A37-8FCD-E5CCE621892D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1One2Manies { get; set; }

        #region Allors
        [Id("046F211E-6791-4F15-ADA0-BC671DEA46E7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1One2One { get; set; }

        #region Allors
        [Id("FCB22EB5-9FA5-4040-9BE7-B994C18EE3FC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2[] SessionSI2Many2Manies { get; set; }

        #region Allors
        [Id("885A9B5E-6F52-4DF6-AF46-0572E70B219A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2Many2One { get; set; }

        #region Allors
        [Id("1E5081F4-A651-4DD1-B580-2D6FEA15085C")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public SI2[] SessionSI2One2Manies { get; set; }

        #region Allors
        [Id("280E08D1-85B2-4AEB-9D3E-519AAF2C0FF6")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2One2One { get; set; }
        #endregion

        #region Workspace Role
        #region Allors
        [Id("90B01B2C-5A12-4D20-B36B-106C951E64BE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1Many2Manies { get; set; }

        #region Allors
        [Id("7C34DB94-600C-42E4-9A64-C1CF788019A0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1Many2One { get; set; }

        #region Allors
        [Id("EC3F3E21-D52D-4997-B6E8-ABEC59DF676D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1One2Manies { get; set; }

        #region Allors
        [Id("2FE469CA-B4C9-4402-8ECF-8D9A41630DD8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1One2One { get; set; }

        #region Allors
        [Id("9F965080-1D3F-4CF0-8EDD-CB86493BAE01")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2Many2Manies { get; set; }

        #region Allors
        [Id("4112F139-9771-46FD-A935-2681CFED4C55")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2Many2One { get; set; }

        #region Allors
        [Id("40B600C5-E098-429D-8D65-B2A42B99FED9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2One2Manies { get; set; }

        #region Allors
        [Id("D3965A2F-637F-4522-B8A9-A7EF9B20C0C4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2One2One { get; set; }

        #region Allors
        [Id("82FE2A04-B0A3-4468-865D-516055099A8E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12Many2Manies { get; set; }

        #region Allors
        [Id("7A62CA41-7C32-44C4-ADB2-51BD2666FEBF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12Many2One { get; set; }

        #region Allors
        [Id("5F8420C5-EA81-4D90-B39D-4E7CB0A2D56A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12One2Manies { get; set; }

        #region Allors
        [Id("4EBFE2B0-0505-449B-98D6-EB4CA5E2DEC1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12One2One { get; set; }

        #region Allors
        [Id("F6A62E0E-D904-480C-95B8-158791D79098")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1Many2Manies { get; set; }

        #region Allors
        [Id("DA0DF04A-BA29-4184-86E5-D0E341E12315")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1Many2One { get; set; }

        #region Allors
        [Id("427BF5EE-52C6-4C41-BF86-46AFD77C1E2D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1One2Manies { get; set; }

        #region Allors
        [Id("0EF4E5B6-9107-4CFE-9AE2-72B13ED0EA26")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1One2One { get; set; }

        #region Allors
        [Id("3B7A74AD-CC94-4845-8CFD-F6D89E7729EB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2[] SessionWI2Many2Manies { get; set; }

        #region Allors
        [Id("63C07B77-324F-41EB-AD54-B7AE13B513A0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2Many2One { get; set; }

        #region Allors
        [Id("038EB13B-E613-475D-8862-BAC87813000A")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] SessionWI2One2Manies { get; set; }

        #region Allors
        [Id("38F4FEFF-AED0-4349-A2E5-821152BC85E7")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2One2One { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("AD5CB186-DBDF-495D-A495-E722AA0061B5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1Many2Manies { get; set; }

        #region Allors
        [Id("7F0ABE52-246B-48CD-940D-14CD8A6CA52D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1Many2One { get; set; }

        #region Allors
        [Id("FA80D4A0-A5B5-4621-AC17-A0E84B757B8E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1One2Manies { get; set; }

        #region Allors
        [Id("AD07CB9C-482D-48F9-83DE-93FA1010CD3E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1One2One { get; set; }

        #region Allors
        [Id("334402A5-15BB-4913-BCBC-6207D47E7A03")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("60B4C3F2-1714-4809-932A-CFF96CF59F6F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 WSessionC2Many2One { get; set; }

        #region Allors
        [Id("2C19F969-BB3A-458D-9024-CF97F93060EE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2One2Manies { get; set; }

        #region Allors
        [Id("D0362614-896E-4CF5-8E18-5A0F45D4F5E8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SessionC2One2One { get; set; }

        #region Allors
        [Id("6398BEEA-726E-4E30-B76D-A73A87AE5ACF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("5820695F-5401-4BC9-B51A-D8F23F1DEAC8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12Many2One { get; set; }

        #region Allors
        [Id("8E8B416A-5E6E-4545-B521-7BAF77CD79AC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12One2Manies { get; set; }

        #region Allors
        [Id("00EDB77F-A0DC-4552-BBEF-2C7EE70B9956")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12One2One { get; set; }

        #region Allors
        [Id("8BAFADE5-4882-4D08-911F-919240944300")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1Many2Manies { get; set; }

        #region Allors
        [Id("4723BB6F-0E76-445A-991B-1B1A11E8C9A6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1Many2One { get; set; }

        #region Allors
        [Id("FC9BEED6-2774-45D8-B4D8-C6E6A8D1298D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1One2Manies { get; set; }

        #region Allors
        [Id("75733BB5-6348-417E-94AA-25D73FF123AA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1One2One { get; set; }

        #region Allors
        [Id("07DF338F-744B-49A8-9D32-8AA248133AEB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2[] SessionI2Many2Manies { get; set; }

        #region Allors
        [Id("446392FA-CFD3-4AA5-96E1-C97F2F7A9679")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2Many2One { get; set; }

        #region Allors
        [Id("045C5D4A-02CC-4911-B546-7C8B4EA2C7C0")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] SessionI2One2Manies { get; set; }

        #region Allors
        [Id("F3F422E5-8618-4F10-A30B-7EDFD68FB679")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2One2One { get; set; }
        #endregion
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void InterfaceMethod() { }

        public void SuperinterfaceMethod() { }

        #endregion
    }
}
