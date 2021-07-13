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
    [Id("7041c691-d896-4628-8f50-1c24f5d03414")]
    #endregion
    [Workspace(Default)]
    public partial class SessionC1 : SessionI1, DerivationCounted, Object
    {
        #region inherited properties
        public I1 SessionI1I1Many2One { get; set; }

        public I12[] SessionI1I12Many2Manies { get; set; }

        public I2[] SessionI1I2Many2Manies { get; set; }

        public I2 SessionI1I2Many2One { get; set; }

        public string SessionI1AllorsString { get; set; }

        public I12 SessionI1I12Many2One { get; set; }

        public DateTime SessionI1AllorsDateTime { get; set; }

        public I2[] SessionI1I2One2Manies { get; set; }

        public C2[] SessionI1C2One2Manies { get; set; }

        public C1 SessionI1C1One2One { get; set; }

        public int SessionI1AllorsInteger { get; set; }

        public C2[] SessionI1C2Many2Manies { get; set; }

        public I1[] SessionI1I1One2Manies { get; set; }

        public I1[] SessionI1I1Many2Manies { get; set; }

        public bool SessionI1AllorsBoolean { get; set; }

        public decimal SessionI1AllorsDecimal { get; set; }

        public I12 SessionI1I12One2One { get; set; }

        public I2 SessionI1I2One2One { get; set; }

        public C2 SessionI1C2One2One { get; set; }

        public C1[] SessionI1C1One2Manies { get; set; }

        public byte[] SessionI1AllorsBinary { get; set; }

        public C1[] SessionI1C1Many2Manies { get; set; }

        public double SessionI1AllorsDouble { get; set; }

        public I1 SessionI1I1One2One { get; set; }

        public C1 SessionI1C1Many2One { get; set; }

        public I12[] SessionI1I12One2Manies { get; set; }

        public C2 SessionI1C2Many2One { get; set; }

        public Guid SessionI1AllorsUnique { get; set; }

        public byte[] SessionI12AllorsBinary { get; set; }

        public C2 SessionI12C2One2One { get; set; }

        public double SessionI12AllorsDouble { get; set; }

        public I1 SessionI12I1Many2One { get; set; }

        public string SessionI12AllorsString { get; set; }

        public I12[] SessionI12I12Many2Manies { get; set; }

        public decimal SessionI12AllorsDecimal { get; set; }

        public I2[] SessionI12I2Many2Manies { get; set; }

        public C2[] SessionI12C2Many2Manies { get; set; }

        public I1[] SessionI12I1Many2Manies { get; set; }

        public I12[] SessionI12I12One2Manies { get; set; }

        public string SessionName { get; set; }

        public C1[] SessionI12C1Many2Manies { get; set; }

        public I2 SessionI12I2Many2One { get; set; }

        public Guid SessionI12AllorsUnique { get; set; }

        public int SessionI12AllorsInteger { get; set; }

        public I1[] SessionI12I1One2Manies { get; set; }

        public C1 SessionI12C1One2One { get; set; }

        public I12 SessionI12I12One2One { get; set; }

        public I2 SessionI12I2One2One { get; set; }

        public I12[] SessionDependencies { get; set; }

        public I2[] SessionI12I2One2Manies { get; set; }

        public C2 SessionI12C2Many2One { get; set; }

        public I12 SessionI12I12Many2One { get; set; }

        public bool SessionI12AllorsBoolean { get; set; }

        public I1 SessionI12I1One2One { get; set; }

        public C1[] SessionI12C1One2Manies { get; set; }

        public C1 SessionI12C1Many2One { get; set; }

        public DateTime SessionI12AllorsDateTime { get; set; }

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

        #region Allors
        [Id("b945b9c8-7cbd-4d91-a215-c7f1ba8a2bf1")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SessionC1AllorsBinary { get; set; }

        #region Allors
        [Id("b1ae5223-1ae4-47be-8091-056328545ac2")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SessionC1AllorsBoolean { get; set; }

        #region Allors
        [Id("9dc16a90-8eed-45ab-a1bf-4b3708a3178c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1AllorsDateTime { get; set; }

        #region Allors
        [Id("a7f7e0af-3df5-4ae7-b7db-b7478040f435")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("cc75ab5e-0e2b-41fc-94f9-0bb8582d693b")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("c4821209-8375-4549-b112-4a77d6ad3132")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("d0d289a1-dea4-4c3e-a882-39c3ca4fc52e")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("34ac5259-21af-424a-aa02-d606c71b7be3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SessionC1AllorsDecimal { get; set; }

        #region Allors
        [Id("cde4a879-2c14-4edb-a9e2-1b5225baad9d")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionC1DecimalLessThan { get; set; }

        #region Allors
        [Id("debdf971-6bd1-434d-b578-820032ce67a0")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("7777f591-338c-47da-97a3-6f171b072fac")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("3abc2c51-fbd7-4771-b8be-2c94697c3082")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("0cd84a51-5edf-4941-a244-ce0152216794")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SessionC1AllorsDouble { get; set; }

        #region Allors
        [Id("f04da0a1-3a7b-4f15-b249-74ace38503c7")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionC1DoubleLessThan { get; set; }

        #region Allors
        [Id("975c9622-c3d0-4d6d-b7b5-a49c128097ed")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("6c6d5840-c449-4f5e-9a73-ecb7c334dc36")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("66bc84d8-e6ff-4f41-9fe7-ae3788985619")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("417f172f-a0ec-46f4-8d01-b116892b1346")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1AllorsInteger { get; set; }

        #region Allors
        [Id("b6d84bad-7777-4450-ae49-7c783469ecbc")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1IntegerLessThan { get; set; }

        #region Allors
        [Id("7b2ffca8-9f0b-4fd0-b771-3f7481739274")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("30a71d66-e02c-4310-8ca1-ff8d11feee88")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("ceb17d79-b3fb-48e3-ac53-7e20cf6b3369")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("6b3db56f-3de6-4b59-83e9-fc370d5544a6")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionC1AllorsString { get; set; }

        #region Allors
        [Id("f1b82206-4165-4167-b0f2-5c99daa37e9b")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionC1AllorsStringEquals { get; set; }

        #region Allors
        [Id("93ddaa00-c1fc-49a1-87f2-9ccb7469c986")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringMax { get; set; }

        #region Allors
        [Id("3b6223e9-d742-4239-9ee5-efc4b98fc679")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid SessionC1AllorsUnique { get; set; }

        #region Allors
        [Id("fc2b012e-b66b-4af6-9fca-443215b2900d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1C1Many2Manies { get; set; }

        #region Allors
        [Id("62cf9aad-a931-4ff8-92b1-00d41eeacbcc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1C1Many2One { get; set; }

        #region Allors
        [Id("0b7e78bc-6186-45ae-9efb-bd39b74f32c4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1C1One2Manies { get; set; }

        #region Allors
        [Id("a72e29fe-823c-4339-84b8-6acbf089d975")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1C1One2One { get; set; }

        #region Allors
        [Id("9970f4d7-1ce7-45ae-addd-041888da3648")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC1C2Many2Manies { get; set; }

        #region Allors
        [Id("12eb72ea-f446-4c00-afd4-1511e7a8c5d3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SessionC1C2Many2One { get; set; }

        #region Allors
        [Id("6704414f-71a9-4024-9ce1-1939309fe0e2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC1C2One2Manies { get; set; }

        #region Allors
        [Id("e74c7ce7-d3f8-49de-8e72-af6e093841ea")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SessionC1C2One2One { get; set; }

        #region Allors
        [Id("b7895ab7-ee91-458a-b138-b9f4dd54f883")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionC1I12Many2Manies { get; set; }

        #region Allors
        [Id("5def23c2-18fe-419a-8197-342f8fcd39b3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionC1I12Many2One { get; set; }

        #region Allors
        [Id("e173bde4-1935-4764-89df-8d5ef7e1c1a5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionC1I12One2Manies { get; set; }

        #region Allors
        [Id("bb84f565-f37c-4e51-9aad-965da88b6874")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionC1I12One2One { get; set; }

        #region Allors
        [Id("7a069ba3-a077-43dc-a844-3b06e7d424d1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionC1I1Many2Manies { get; set; }

        #region Allors
        [Id("195b47f6-2d75-477e-97f7-b2ef495b552b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionC1I1Many2One { get; set; }

        #region Allors
        [Id("b671c5cd-8b46-49a1-bfec-324a5bb733d7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionC1I1One2Manies { get; set; }

        #region Allors
        [Id("846be756-aa06-45b8-ac2a-4a6d82fc7526")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionC1I1One2One { get; set; }

        #region Allors
        [Id("96201daa-cbf4-4a30-8235-45f28f9d4b5a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2[] SessionC1I2Many2Manies { get; set; }

        #region Allors
        [Id("565695d2-b67a-48e7-9103-4a64fe6b89fe")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionC1I2Many2One { get; set; }

        #region Allors
        [Id("ccb8ecda-351d-4204-8863-0029971788c6")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] SessionC1I2One2Manies { get; set; }

        #region Allors
        [Id("42fe3e7f-2ac9-4455-bb42-7affcbf71534")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionC1I2One2One { get; set; }

        #region Allors
        [Id("cd580c5e-e28f-43e3-a4a8-aae69e4bed02")]
        #endregion
        [Workspace(Default)]
        public void ClassMethod() { }

        #region Allors
        [Id("326ce858-01bf-43c6-87bb-9c163e1e725b")]
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
