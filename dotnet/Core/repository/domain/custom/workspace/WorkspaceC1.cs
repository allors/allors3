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
    public partial class WorkspaceC1 : WorkspaceI1, DerivationCounted, Object
    {
        #region inherited properties
        public I1 WorkspaceI1I1Many2One { get; set; }

        public I12[] WorkspaceI1I12Many2Manies { get; set; }

        public I2[] WorkspaceI1I2Many2Manies { get; set; }

        public I2 WorkspaceI1I2Many2One { get; set; }

        public string WorkspaceI1AllorsString { get; set; }

        public I12 WorkspaceI1I12Many2One { get; set; }

        public DateTime WorkspaceI1AllorsDateTime { get; set; }

        public I2[] WorkspaceI1I2One2Manies { get; set; }

        public C2[] WorkspaceI1C2One2Manies { get; set; }

        public C1 WorkspaceI1C1One2One { get; set; }

        public int WorkspaceI1AllorsInteger { get; set; }

        public C2[] WorkspaceI1C2Many2Manies { get; set; }

        public I1[] WorkspaceI1I1One2Manies { get; set; }

        public I1[] WorkspaceI1I1Many2Manies { get; set; }

        public bool WorkspaceI1AllorsBoolean { get; set; }

        public decimal WorkspaceI1AllorsDecimal { get; set; }

        public I12 WorkspaceI1I12One2One { get; set; }

        public I2 WorkspaceI1I2One2One { get; set; }

        public C2 WorkspaceI1C2One2One { get; set; }

        public C1[] WorkspaceI1C1One2Manies { get; set; }

        public byte[] WorkspaceI1AllorsBinary { get; set; }

        public C1[] WorkspaceI1C1Many2Manies { get; set; }

        public double WorkspaceI1AllorsDouble { get; set; }

        public I1 WorkspaceI1I1One2One { get; set; }

        public C1 WorkspaceI1C1Many2One { get; set; }

        public I12[] WorkspaceI1I12One2Manies { get; set; }

        public C2 WorkspaceI1C2Many2One { get; set; }

        public Guid WorkspaceI1AllorsUnique { get; set; }

        public byte[] WorkspaceI12AllorsBinary { get; set; }

        public C2 WorkspaceI12C2One2One { get; set; }

        public double WorkspaceI12AllorsDouble { get; set; }

        public I1 WorkspaceI12I1Many2One { get; set; }

        public string WorkspaceI12AllorsString { get; set; }

        public I12[] WorkspaceI12I12Many2Manies { get; set; }

        public decimal WorkspaceI12AllorsDecimal { get; set; }

        public I2[] WorkspaceI12I2Many2Manies { get; set; }

        public C2[] WorkspaceI12C2Many2Manies { get; set; }

        public I1[] WorkspaceI12I1Many2Manies { get; set; }

        public I12[] WorkspaceI12I12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public C1[] WorkspaceI12C1Many2Manies { get; set; }

        public I2 WorkspaceI12I2Many2One { get; set; }

        public Guid WorkspaceI12AllorsUnique { get; set; }

        public int WorkspaceI12AllorsInteger { get; set; }

        public I1[] WorkspaceI12I1One2Manies { get; set; }

        public C1 WorkspaceI12C1One2One { get; set; }

        public I12 WorkspaceI12I12One2One { get; set; }

        public I2 WorkspaceI12I2One2One { get; set; }

        public I12[] WorkspaceDependencies { get; set; }

        public I2[] WorkspaceI12I2One2Manies { get; set; }

        public C2 WorkspaceI12C2Many2One { get; set; }

        public I12 WorkspaceI12I12Many2One { get; set; }

        public bool WorkspaceI12AllorsBoolean { get; set; }

        public I1 WorkspaceI12I1One2One { get; set; }

        public C1[] WorkspaceI12C1One2Manies { get; set; }

        public C1 WorkspaceI12C1Many2One { get; set; }

        public DateTime WorkspaceI12AllorsDateTime { get; set; }

        public I1 DatabaseI1I1Many2One { get; set; }

        public I12[] DatabaseI1I12Many2Manies { get; set; }

        public I2[] DatabaseI1I2Many2Manies { get; set; }

        public I2 DatabaseI1I2Many2One { get; set; }

        public string DatabaseI1AllorsString { get; set; }

        public I12 DatabaseI1I12Many2One { get; set; }

        public DateTime DatabaseI1AllorsDateTime { get; set; }

        public I2[] DatabaseI1I2One2Manies { get; set; }

        public C2[] DatabaseI1C2One2Manies { get; set; }

        public C1 DatabaseI1C1One2One { get; set; }

        public int DatabaseI1AllorsInteger { get; set; }

        public C2[] DatabaseI1C2Many2Manies { get; set; }

        public I1[] DatabaseI1I1One2Manies { get; set; }

        public I1[] DatabaseI1I1Many2Manies { get; set; }

        public bool DatabaseI1AllorsBoolean { get; set; }

        public decimal DatabaseI1AllorsDecimal { get; set; }

        public I12 DatabaseI1I12One2One { get; set; }

        public I2 DatabaseI1I2One2One { get; set; }

        public C2 DatabaseI1C2One2One { get; set; }

        public C1[] DatabaseI1C1One2Manies { get; set; }

        public byte[] DatabaseI1AllorsBinary { get; set; }

        public C1[] DatabaseI1C1Many2Manies { get; set; }

        public double DatabaseI1AllorsDouble { get; set; }

        public I1 DatabaseI1I1One2One { get; set; }

        public C1 DatabaseI1C1Many2One { get; set; }

        public I12[] DatabaseI1I12One2Manies { get; set; }

        public C2 DatabaseI1C2Many2One { get; set; }

        public Guid DatabaseI1AllorsUnique { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 DatabaseI12C2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 DatabaseI12I1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] DatabaseI12I12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] DatabaseI12I2Many2Manies { get; set; }

        public C2[] DatabaseI12C2Many2Manies { get; set; }

        public I1[] DatabaseI12I1Many2Manies { get; set; }

        public I12[] DatabaseI12I12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] DatabaseI12C1Many2Manies { get; set; }

        public I2 DatabaseI12I2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] DatabaseI12I1One2Manies { get; set; }

        public C1 DatabaseI12C1One2One { get; set; }

        public I12 DatabaseI12I12One2One { get; set; }

        public I2 DatabaseI12I2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] DatabaseI12I2One2Manies { get; set; }

        public C2 DatabaseI12C2Many2One { get; set; }

        public I12 DatabaseI12I12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 DatabaseI12I1One2One { get; set; }

        public C1[] DatabaseI12C1One2Manies { get; set; }

        public C1 DatabaseI12C1Many2One { get; set; }

        public DateTime DatabaseI12AllorsDateTime { get; set; }


        public Permission[] DeniedPermissions { get; set; }

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

        #region Workspace

        #region Allors
        [Id("b1168c3c-6379-4824-86c4-1f5ad22fd53c")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public byte[] WorkspaceC1AllorsBinary { get; set; }

        #region Allors
        [Id("5d2437da-20f5-48a4-938a-c00d3a6e479c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public bool WorkspaceC1AllorsBoolean { get; set; }

        #region Allors
        [Id("0e99f73f-3360-4aae-865b-f4ee6d58b136")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1AllorsDateTime { get; set; }

        #region Allors
        [Id("b2b24f60-f865-4e9c-8a62-d35a5ac86fb0")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("89939dc5-99d6-4bb5-a1d9-260c026f02df")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("23b405e3-f1a0-4898-a7e8-75a5f1e92285")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("9a0e8019-54bf-4521-ae53-76ca43e00338")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("37dae9c1-6a1b-4d6f-bd25-48a3b2bb8d74")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public decimal WorkspaceC1AllorsDecimal { get; set; }

        #region Allors
        [Id("9dfb0777-51bf-4906-92a8-bad22dc12c2f")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceC1DecimalLessThan { get; set; }

        #region Allors
        [Id("5154f91c-7c7d-433d-90f1-1f7d07f74d1d")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("500e4d60-ca74-4ed2-8e62-198a44899236")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("ad3ce131-e19c-4902-9c77-f3a2abc7eb43")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("6b5971b4-4d9f-4a9d-b2c0-6e5f2f372333")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public double WorkspaceC1AllorsDouble { get; set; }

        #region Allors
        [Id("c6d5fa47-913d-41dc-96b2-e7f16d0453a9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceC1DoubleLessThan { get; set; }

        #region Allors
        [Id("eed21466-9bd4-4faf-8729-e90af9eff00a")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("0eba124c-93cb-481d-a542-76c740ab30cc")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("8a3e4e87-14db-4448-be59-8908d76fbc91")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("b4666eec-97bd-4f39-aafc-8fd6ee485c80")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1AllorsInteger { get; set; }

        #region Allors
        [Id("8482d1d2-4787-484e-968d-429f58156cc1")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1IntegerLessThan { get; set; }

        #region Allors
        [Id("aebfcffb-b5bc-48a8-853a-01e5433b353d")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("9ff46560-bc8b-4c7f-afc0-2cf4968bbfa5")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("455a02c0-aaff-4c62-8d7a-551e63a1c117")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("b2b5703f-7164-4ea3-8d31-6f13c1836942")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceC1AllorsString { get; set; }

        #region Allors
        [Id("d9fa46d0-1868-4f4c-bffa-87afeb82b21a")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceC1AllorsStringEquals { get; set; }

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
        public Guid WorkspaceC1AllorsUnique { get; set; }

        #region Allors
        [Id("59c35948-3a12-46f2-b378-251c36f60d40")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1C1Many2Manies { get; set; }

        #region Allors
        [Id("13a1ac52-cbd3-4219-b3e1-7c7fdcdf90e2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1C1Many2One { get; set; }

        #region Allors
        [Id("fb60c9ae-0a9e-4fa2-8fa2-d5a6960bcfe1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1C1One2Manies { get; set; }

        #region Allors
        [Id("d4404cb1-c9b9-4ebe-9568-8dddc0a9e482")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1C1One2One { get; set; }

        #region Allors
        [Id("c7a013df-c73a-4878-911b-7c17eae52bfb")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC1C2Many2Manies { get; set; }

        #region Allors
        [Id("af8fdd90-5288-47bd-9df8-83a410836bde")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WorkspaceC1C2Many2One { get; set; }

        #region Allors
        [Id("bab4db1f-8c83-4c78-b268-6b79c1649266")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC1C2One2Manies { get; set; }

        #region Allors
        [Id("f549984d-d894-4bd4-a8b3-b0facfb261d3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WorkspaceC1C2One2One { get; set; }

        #region Allors
        [Id("9f4e49b9-2819-4517-a6a5-38f9c08c5288")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceC1I12Many2Manies { get; set; }

        #region Allors
        [Id("21133757-db64-4096-b939-d52ac6a713ae")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceC1I12Many2One { get; set; }

        #region Allors
        [Id("649d7d79-5a85-4d2a-8604-92df80d1f532")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceC1I12One2Manies { get; set; }

        #region Allors
        [Id("053edc8b-a580-4c57-b040-7345ac8f4d95")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceC1I12One2One { get; set; }

        #region Allors
        [Id("b3dbe29e-8782-45f7-b3c3-5a4a3c9b127f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceC1I1Many2Manies { get; set; }

        #region Allors
        [Id("0df01347-03d5-467b-a7a7-fb4114964f6c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceC1I1Many2One { get; set; }

        #region Allors
        [Id("322c8a86-ea0a-4f09-844b-94f714e2e7f3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceC1I1One2Manies { get; set; }

        #region Allors
        [Id("ccb4599d-512d-4868-b0d2-d4c49e9101ac")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceC1I1One2One { get; set; }

        #region Allors
        [Id("d62c480e-4803-47ec-94cf-f19c21357f22")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2[] WorkspaceC1I2Many2Manies { get; set; }

        #region Allors
        [Id("635ef67a-65ca-4c5b-b8d3-cc0009317cd5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 WorkspaceC1I2Many2One { get; set; }

        #region Allors
        [Id("f7274131-b157-4e6e-bd20-20273e098cbd")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] WorkspaceC1I2One2Manies { get; set; }

        #region Allors
        [Id("a501851a-b485-440b-a371-ac1699f97821")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 WorkspaceC1I2One2One { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("897cf791-5e8b-4b55-966c-ff521f64c3e5")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public byte[] DatabaseC1AllorsBinary { get; set; }

        #region Allors
        [Id("87a6c554-957b-41ee-8f6d-3d70b53192b3")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public bool DatabaseC1AllorsBoolean { get; set; }

        #region Allors
        [Id("8cab47af-9fc9-4f2e-a644-b9a682bec0f5")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime DatabaseC1AllorsDateTime { get; set; }

        #region Allors
        [Id("ae838733-7fa4-48b1-a03d-e33aca724c05")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime DatabaseC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("3d632cd9-8173-42af-90e5-892607289fa3")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime DatabaseC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("62b5a6c6-bd2a-40ed-9966-4a4904d7a4f9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime DatabaseC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("424f74d8-4b8b-4f9c-9aa3-1fab43fe6782")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime DatabaseC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("98960d21-b25c-472d-9d64-c07be6626630")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public decimal DatabaseC1AllorsDecimal { get; set; }

        #region Allors
        [Id("01da9444-1260-4c80-8a3f-500fe75f4e30")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal DatabaseC1DecimalLessThan { get; set; }

        #region Allors
        [Id("91d55f31-d5a2-485e-949c-9f540e57e903")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal DatabaseC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("e2eb5a09-11c8-4560-b8c0-677b89b9bffe")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal DatabaseC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("dc3e2386-53cb-420b-b318-f900d7c0593c")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal DatabaseC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("ab0fa2a2-4463-434b-a067-d46252206454")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public double DatabaseC1AllorsDouble { get; set; }

        #region Allors
        [Id("fba7d752-2afa-407c-af8c-3887926c1d9c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double DatabaseC1DoubleLessThan { get; set; }

        #region Allors
        [Id("3c72c2ec-f9e8-4b55-980e-f9999e2c848f")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double DatabaseC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("7329a0f2-a526-4c3b-8899-17e402bc0301")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double DatabaseC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("bbaf1aea-3747-4692-9976-1193dfd5acc3")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double DatabaseC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("c69a6ec7-8f41-4249-a061-0d62413e9887")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int DatabaseC1AllorsInteger { get; set; }

        #region Allors
        [Id("a2768643-54d4-4dc7-b2b7-febd12dd9991")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int DatabaseC1IntegerLessThan { get; set; }

        #region Allors
        [Id("9b1b81ba-b949-4bff-bf7f-8397576ac964")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int DatabaseC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("04c398e9-e1d8-47bd-a5d0-a8ca373be458")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int DatabaseC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("5387cfc0-0ccc-435c-a7ad-b61ad9b04d96")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int DatabaseC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("1b92e4a1-dbb3-41ee-a3e5-f82dcdc9f3eb")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string DatabaseC1AllorsString { get; set; }

        #region Allors
        [Id("3fe37556-92bb-4243-8995-51848a52ac2f")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string DatabaseC1AllorsStringEquals { get; set; }

        #region Allors
        [Id("17feb068-fbe9-4efa-a875-e5de62e4f086")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string DatabaseAllorsStringMax { get; set; }

        #region Allors
        [Id("f9cf823d-5f4e-4974-bdc9-81231ae6abc6")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Guid DatabaseC1AllorsUnique { get; set; }

        #region Allors
        [Id("09f2203a-e609-480f-899e-7233004205c1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] DatabaseC1C1Many2Manies { get; set; }

        #region Allors
        [Id("1e3f7088-55d5-4e07-91c9-96e08c06d826")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 DatabaseC1C1Many2One { get; set; }

        #region Allors
        [Id("05e5b19a-ac7f-49dd-be46-ab5a27f83b8e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] DatabaseC1C1One2Manies { get; set; }

        #region Allors
        [Id("b989909b-22b0-4355-a3c8-f769beb0528c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 DatabaseC1C1One2One { get; set; }

        #region Allors
        [Id("45491d2d-ba05-43ad-ac8b-c9c4468556af")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] DatabaseC1C2Many2Manies { get; set; }

        #region Allors
        [Id("a583ad5e-d6a5-43de-b8f7-c2a0de0d629e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 DatabaseC1C2Many2One { get; set; }

        #region Allors
        [Id("3764a1a2-94bc-45bf-be19-030956dbd819")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] DatabaseC1C2One2Manies { get; set; }

        #region Allors
        [Id("321b9054-b325-4be0-890a-5af5696f68b6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 DatabaseC1C2One2One { get; set; }

        #region Allors
        [Id("21278eee-e78a-494d-b062-a16cba401fd8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] DatabaseC1I12Many2Manies { get; set; }

        #region Allors
        [Id("64da3515-fa2a-4121-b4a7-151e85da423b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 DatabaseC1I12Many2One { get; set; }

        #region Allors
        [Id("d0d85fbe-5815-4cdd-b6a9-b519a5ed58b5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] DatabaseC1I12One2Manies { get; set; }

        #region Allors
        [Id("09cb1c23-1b2c-47e0-806c-f5739b8f74a9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 DatabaseC1I12One2One { get; set; }

        #region Allors
        [Id("5253065d-9253-41fa-afc8-b6b9f4bfe325")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] DatabaseC1I1Many2Manies { get; set; }

        #region Allors
        [Id("b57a38db-69d2-4837-9e3a-afb85dbbe6ab")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 DatabaseC1I1Many2One { get; set; }

        #region Allors
        [Id("65670bd0-f3a2-46d7-902c-08d5380c7fa2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] DatabaseC1I1One2Manies { get; set; }

        #region Allors
        [Id("1800ec28-b5dc-43a5-bece-919b6ca24da0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 DatabaseC1I1One2One { get; set; }

        #region Allors
        [Id("86208458-2000-40a7-aba9-2d60f1310413")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2[] DatabaseC1I2Many2Manies { get; set; }

        #region Allors
        [Id("45929620-ce95-4dfb-b4ed-005266e40a5e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 DatabaseC1I2Many2One { get; set; }

        #region Allors
        [Id("0b39c9cd-923e-4024-aa93-6e02d71bf567")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] DatabaseC1I2One2Manies { get; set; }

        #region Allors
        [Id("3567d86a-e3cb-4bbe-aef1-0b30fa18161e")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 DatabaseC1I2One2One { get; set; }

        #endregion

        #region Allors
        [Id("d208a271-ff51-462d-badb-0f687739bb31")]
        #endregion
        [Workspace(Default)]
        public void ClassMethod() { }

        #region Allors
        [Id("0f89c81f-d4d7-45f8-8a7d-274d609cc6c8")]
        #endregion
        public void Sum() { }

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
