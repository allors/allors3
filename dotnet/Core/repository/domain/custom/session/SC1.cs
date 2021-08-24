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
    [Id("65e0ac1c-a1bc-4ca6-be5f-5e2a5e4bf9f0")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Session)]
    public partial class SC1 : SI1, DerivationCounted, Object
    {
        #region inherited properties
        public SI1 SI1SI1Many2One { get; set; }

        public SI12[] SI1SI12Many2Manies { get; set; }

        public SI2[] SI1SI2Many2Manies { get; set; }

        public SI2 SI1SI2Many2One { get; set; }

        public string SI1AllorsString { get; set; }

        public SI12 SI1SI12Many2One { get; set; }

        public DateTime SI1AllorsDateTime { get; set; }

        public SI2[] SI1SI2One2Manies { get; set; }

        public SC2[] SI1SC2One2Manies { get; set; }

        public SC1 SI1SC1One2One { get; set; }

        public int SI1AllorsInteger { get; set; }

        public SC2[] SI1SC2Many2Manies { get; set; }

        public SI1[] SI1SI1One2Manies { get; set; }

        public SI1[] SI1SI1Many2Manies { get; set; }

        public bool SI1AllorsBoolean { get; set; }

        public decimal SI1AllorsDecimal { get; set; }

        public SI12 SI1SI12One2One { get; set; }

        public SI2 SI1SI2One2One { get; set; }

        public SC2 SI1SC2One2One { get; set; }

        public SC1[] SI1SC1One2Manies { get; set; }

        public byte[] SI1AllorsBinary { get; set; }

        public SC1[] SI1SC1Many2Manies { get; set; }

        public double SI1AllorsDouble { get; set; }

        public SI1 SI1SI1One2One { get; set; }

        public SC1 SI1SC1Many2One { get; set; }

        public SI12[] SI1SI12One2Manies { get; set; }

        public SC2 SI1SC2Many2One { get; set; }

        public Guid SI1AllorsUnique { get; set; }

        public byte[] SI12AllorsBinary { get; set; }

        public SC2 SI12SC2One2One { get; set; }

        public double SI12AllorsDouble { get; set; }

        public SI1 SI12SI1Many2One { get; set; }

        public string SI12AllorsString { get; set; }

        public SI12[] SI12SI12Many2Manies { get; set; }

        public decimal SI12AllorsDecimal { get; set; }

        public SI2[] SI12SI2Many2Manies { get; set; }

        public SC2[] SI12SC2Many2Manies { get; set; }

        public SI1[] SI12SI1Many2Manies { get; set; }

        public SI12[] SI12SI12One2Manies { get; set; }

        public string SessionName { get; set; }

        public SC1[] SI12SC1Many2Manies { get; set; }

        public SI2 SI12SI2Many2One { get; set; }

        public Guid SI12AllorsUnique { get; set; }

        public int SI12AllorsInteger { get; set; }

        public SI1[] SI12SI1One2Manies { get; set; }

        public SC1 SI12SC1One2One { get; set; }

        public SI12 SI12SI12One2One { get; set; }

        public SI2 SI12SI2One2One { get; set; }

        public SI12[] SessionDependencies { get; set; }

        public SI2[] SI12SI2One2Manies { get; set; }

        public SC2 SI12SC2Many2One { get; set; }

        public SI12 SI12SI12Many2One { get; set; }

        public bool SI12AllorsBoolean { get; set; }

        public SI1 SI12SI1One2One { get; set; }

        public SC1[] SI12SC1One2Manies { get; set; }

        public SC1 SI12SC1Many2One { get; set; }

        public DateTime SI12AllorsDateTime { get; set; }

        public WI1 SI1WI1Many2One { get; set; }

        public WI12[] SI1WI12Many2Manies { get; set; }

        public WI2[] SI1WI2Many2Manies { get; set; }

        public WI2 SI1WI2Many2One { get; set; }

        public string WI1AllorsString { get; set; }

        public WI12 SI1WI12Many2One { get; set; }

        public DateTime WI1AllorsDateTime { get; set; }

        public WI2[] SI1WI2One2Manies { get; set; }

        public WC2[] SI1WC2One2Manies { get; set; }

        public WC1 SI1WC1One2One { get; set; }

        public int WI1AllorsInteger { get; set; }

        public WC2[] SI1WC2Many2Manies { get; set; }

        public WI1[] SI1WI1One2Manies { get; set; }

        public WI1[] SI1WI1Many2Manies { get; set; }

        public bool WI1AllorsBoolean { get; set; }

        public decimal WI1AllorsDecimal { get; set; }

        public WI12 SI1WI12One2One { get; set; }

        public WI2 SI1WI2One2One { get; set; }

        public WC2 SI1WC2One2One { get; set; }

        public WC1[] SI1WC1One2Manies { get; set; }

        public byte[] WI1AllorsBinary { get; set; }

        public WC1[] SI1WC1Many2Manies { get; set; }

        public double WI1AllorsDouble { get; set; }

        public WI1 SI1WI1One2One { get; set; }

        public WC1 SI1WC1Many2One { get; set; }

        public WI12[] SI1WI12One2Manies { get; set; }

        public WC2 SI1WC2Many2One { get; set; }

        public Guid WI1AllorsUnique { get; set; }

        public byte[] WI12AllorsBinary { get; set; }

        public WC2 SI12WC2One2One { get; set; }

        public double WI12AllorsDouble { get; set; }

        public WI1 SI12WI1Many2One { get; set; }

        public string WI12AllorsString { get; set; }

        public WI12[] SI12WI12Many2Manies { get; set; }

        public decimal WI12AllorsDecimal { get; set; }

        public WI2[] SI12WI2Many2Manies { get; set; }

        public WC2[] SI12WC2Many2Manies { get; set; }

        public WI1[] SI12WI1Many2Manies { get; set; }

        public WI12[] SI12WI12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public WC1[] SI12WC1Many2Manies { get; set; }

        public WI2 SI12WI2Many2One { get; set; }

        public Guid WI12AllorsUnique { get; set; }

        public int WI12AllorsInteger { get; set; }

        public WI1[] SI12WI1One2Manies { get; set; }

        public WC1 SI12WC1One2One { get; set; }

        public WI12 SI12WI12One2One { get; set; }

        public WI2 SI12WI2One2One { get; set; }

        public WI12[] WorkspaceDependencies { get; set; }

        public WI2[] SI12WI2One2Manies { get; set; }

        public WC2 SI12WC2Many2One { get; set; }

        public WI12 SI12WI12Many2One { get; set; }

        public bool WI12AllorsBoolean { get; set; }

        public WI1 SI12WI1One2One { get; set; }

        public WC1[] SI12WC1One2Manies { get; set; }

        public WC1 SI12WC1Many2One { get; set; }

        public DateTime WI12AllorsDateTime { get; set; }

        public I1 SI1DatabaseI1Many2One { get; set; }

        public I12[] SI1DatabaseI12Many2Manies { get; set; }

        public I2[] SI1DatabaseI2Many2Manies { get; set; }

        public I2 SI1DatabaseI2Many2One { get; set; }

        public string DatabaseI1AllorsString { get; set; }

        public I12 SI1DatabaseI12Many2One { get; set; }

        public DateTime DatabaseI1AllorsDateTime { get; set; }

        public I2[] SI1DatabaseI2One2Manies { get; set; }

        public C2[] SI1DatabaseC2One2Manies { get; set; }

        public C1 SI1DatabaseC1One2One { get; set; }

        public int DatabaseI1AllorsInteger { get; set; }

        public C2[] SI1DatabaseC2Many2Manies { get; set; }

        public I1[] SI1DatabaseI1One2Manies { get; set; }

        public I1[] SI1DatabaseI1Many2Manies { get; set; }

        public bool DatabaseI1AllorsBoolean { get; set; }

        public decimal DatabaseI1AllorsDecimal { get; set; }

        public I12 SI1DatabaseI12One2One { get; set; }

        public I2 SI1DatabaseI2One2One { get; set; }

        public C2 SI1DatabaseC2One2One { get; set; }

        public C1[] SI1DatabaseC1One2Manies { get; set; }

        public byte[] DatabaseI1AllorsBinary { get; set; }

        public C1[] SI1DatabaseC1Many2Manies { get; set; }

        public double DatabaseI1AllorsDouble { get; set; }

        public I1 SI1DatabaseI1One2One { get; set; }

        public C1 SI1DatabaseC1Many2One { get; set; }

        public I12[] SI1DatabaseI12One2Manies { get; set; }

        public C2 SI1DatabaseC2Many2One { get; set; }

        public Guid DatabaseI1AllorsUnique { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 SI12DatabaseC2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 SI12DatabaseI1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] SI12DatabaseI12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] SI12DatabaseI2Many2Manies { get; set; }

        public C2[] SI12DatabaseC2Many2Manies { get; set; }

        public I1[] SI12DatabaseI1Many2Manies { get; set; }

        public I12[] SI12DatabaseI12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] SI12DatabaseC1Many2Manies { get; set; }

        public I2 SI12DatabaseI2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] SI12DatabaseI1One2Manies { get; set; }

        public C1 SI12DatabaseC1One2One { get; set; }

        public I12 SI12DatabaseI12One2One { get; set; }

        public I2 SI12DatabaseI2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] SI12DatabaseI2One2Manies { get; set; }

        public C2 SI12DatabaseC2Many2One { get; set; }

        public I12 SI12DatabaseI12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 SI12DatabaseI1One2One { get; set; }

        public C1[] SI12DatabaseC1One2Manies { get; set; }

        public C1 SI12DatabaseC1Many2One { get; set; }

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

        #region Session

        #region Allors
        [Id("b945b9c8-7cbd-4d91-a215-c7f1ba8a2bf1")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SC1AllorsBinary { get; set; }

        #region Allors
        [Id("b1ae5223-1ae4-47be-8091-056328545ac2")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SC1AllorsBoolean { get; set; }

        #region Allors
        [Id("9dc16a90-8eed-45ab-a1bf-4b3708a3178c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SC1AllorsDateTime { get; set; }

        #region Allors
        [Id("a7f7e0af-3df5-4ae7-b7db-b7478040f435")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("cc75ab5e-0e2b-41fc-94f9-0bb8582d693b")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("c4821209-8375-4549-b112-4a77d6ad3132")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("d0d289a1-dea4-4c3e-a882-39c3ca4fc52e")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("34ac5259-21af-424a-aa02-d606c71b7be3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SC1AllorsDecimal { get; set; }

        #region Allors
        [Id("cde4a879-2c14-4edb-a9e2-1b5225baad9d")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SC1DecimalLessThan { get; set; }

        #region Allors
        [Id("debdf971-6bd1-434d-b578-820032ce67a0")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("7777f591-338c-47da-97a3-6f171b072fac")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("3abc2c51-fbd7-4771-b8be-2c94697c3082")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("0cd84a51-5edf-4941-a244-ce0152216794")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SC1AllorsDouble { get; set; }

        #region Allors
        [Id("f04da0a1-3a7b-4f15-b249-74ace38503c7")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SC1DoubleLessThan { get; set; }

        #region Allors
        [Id("975c9622-c3d0-4d6d-b7b5-a49c128097ed")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("6c6d5840-c449-4f5e-9a73-ecb7c334dc36")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("66bc84d8-e6ff-4f41-9fe7-ae3788985619")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("417f172f-a0ec-46f4-8d01-b116892b1346")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SC1AllorsInteger { get; set; }

        #region Allors
        [Id("b6d84bad-7777-4450-ae49-7c783469ecbc")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SC1IntegerLessThan { get; set; }

        #region Allors
        [Id("7b2ffca8-9f0b-4fd0-b771-3f7481739274")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("30a71d66-e02c-4310-8ca1-ff8d11feee88")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("ceb17d79-b3fb-48e3-ac53-7e20cf6b3369")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("6b3db56f-3de6-4b59-83e9-fc370d5544a6")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SC1AllorsString { get; set; }

        #region Allors
        [Id("f1b82206-4165-4167-b0f2-5c99daa37e9b")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SC1AllorsStringEquals { get; set; }

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
        public Guid SC1AllorsUnique { get; set; }

        #region Allors
        [Id("fc2b012e-b66b-4af6-9fca-443215b2900d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SC1SC1Many2Manies { get; set; }

        #region Allors
        [Id("62cf9aad-a931-4ff8-92b1-00d41eeacbcc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SC1SC1Many2One { get; set; }

        #region Allors
        [Id("0b7e78bc-6186-45ae-9efb-bd39b74f32c4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SC1SC1One2Manies { get; set; }

        #region Allors
        [Id("a72e29fe-823c-4339-84b8-6acbf089d975")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SC1SC1One2One { get; set; }

        #region Allors
        [Id("9970f4d7-1ce7-45ae-addd-041888da3648")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SC1SC2Many2Manies { get; set; }

        #region Allors
        [Id("12eb72ea-f446-4c00-afd4-1511e7a8c5d3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SC1SC2Many2One { get; set; }

        #region Allors
        [Id("6704414f-71a9-4024-9ce1-1939309fe0e2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SC1SC2One2Manies { get; set; }

        #region Allors
        [Id("e74c7ce7-d3f8-49de-8e72-af6e093841ea")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SC1SC2One2One { get; set; }

        #region Allors
        [Id("b7895ab7-ee91-458a-b138-b9f4dd54f883")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SC1SI12Many2Manies { get; set; }

        #region Allors
        [Id("5def23c2-18fe-419a-8197-342f8fcd39b3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SC1SI12Many2One { get; set; }

        #region Allors
        [Id("e173bde4-1935-4764-89df-8d5ef7e1c1a5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SC1SI12One2Manies { get; set; }

        #region Allors
        [Id("bb84f565-f37c-4e51-9aad-965da88b6874")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SC1SI12One2One { get; set; }

        #region Allors
        [Id("7a069ba3-a077-43dc-a844-3b06e7d424d1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SC1SI1Many2Manies { get; set; }

        #region Allors
        [Id("195b47f6-2d75-477e-97f7-b2ef495b552b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SC1SI1Many2One { get; set; }

        #region Allors
        [Id("b671c5cd-8b46-49a1-bfec-324a5bb733d7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SC1SI1One2Manies { get; set; }

        #region Allors
        [Id("846be756-aa06-45b8-ac2a-4a6d82fc7526")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SC1SI1One2One { get; set; }

        #region Allors
        [Id("96201daa-cbf4-4a30-8235-45f28f9d4b5a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2[] SC1SI2Many2Manies { get; set; }

        #region Allors
        [Id("565695d2-b67a-48e7-9103-4a64fe6b89fe")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SC1SI2Many2One { get; set; }

        #region Allors
        [Id("ccb8ecda-351d-4204-8863-0029971788c6")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public SI2[] SC1SI2One2Manies { get; set; }

        #region Allors
        [Id("42fe3e7f-2ac9-4455-bb42-7affcbf71534")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SC1SI2One2One { get; set; }

        #endregion

        #region Workspace

        #region Allors
        [Id("622d4335-e1d2-432e-b0f5-56f295ef08e9")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] WC1AllorsBinary { get; set; }

        #region Allors
        [Id("c61d3875-e08b-404c-a8f0-43fdd6af1e56")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool WC1AllorsBoolean { get; set; }

        #region Allors
        [Id("0b27a26d-c8c2-43c7-9d63-37c76fc4d0e0")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime WC1AllorsDateTime { get; set; }

        #region Allors
        [Id("c26e56e3-75d9-4695-aa57-e612016adda2")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime WC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("1a6bc159-49e5-4927-a615-8b1f3013bfef")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime WC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("bf981eba-2169-489e-b47c-a6bd0db76e78")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime WC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("746ce2a3-8021-406a-9d5d-946f7e285062")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime WC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("f01c0f24-cc75-4bf5-a141-6575d40ab20f")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal WC1AllorsDecimal { get; set; }

        #region Allors
        [Id("fbb3c47e-1129-4a43-8783-c5d1868cf98f")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal WC1DecimalLessThan { get; set; }

        #region Allors
        [Id("3ec83d0d-0717-4479-b812-75084df9b3a0")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal WC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("473feb14-6183-4e4c-8fbf-cfbd96c7b3f3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal WC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("bc997657-6530-4dbe-b34e-ee73e525e787")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal WC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("8495ec00-c0f0-4ee1-91d9-1cef49601fd6")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double WC1AllorsDouble { get; set; }

        #region Allors
        [Id("278e2b5f-68e3-45a4-a801-f8ad513c8ffc")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double WC1DoubleLessThan { get; set; }

        #region Allors
        [Id("92b7cae9-2c31-4082-bd76-b9fc9046fb66")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double WC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("5855d8dd-f4e5-4f4f-8583-b9f420f09ee9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double WC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("878c8b48-06f2-4d28-b00a-8e263a9db6ca")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double WC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("9e88951e-c083-4d47-8fd5-4f730b33a6c5")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int WC1AllorsInteger { get; set; }

        #region Allors
        [Id("3cb13cf3-8711-4ed0-91d6-e20cb9eb1621")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int WC1IntegerLessThan { get; set; }

        #region Allors
        [Id("f3309bbb-28e5-4f50-bde0-5c81a52c8c36")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int WC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("b5cbb410-1b4d-4ac3-9b74-1f600379bce4")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int WC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("3032f740-bbde-4c3d-9c7c-d52b751f90cb")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int WC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("cffd7630-e220-4595-b4a5-13e062e22d75")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string WC1AllorsString { get; set; }

        #region Allors
        [Id("91b0aace-2a39-49c2-a6c0-dfc531ef8452")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string WC1AllorsStringEquals { get; set; }

        #region Allors
        [Id("d3e64743-4296-4ebe-b908-71ae97d5c5ca")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string WorkspaceAllorsStringMax { get; set; }

        #region Allors
        [Id("59ae4633-f11b-4f37-859a-a0d1eff82791")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid WC1AllorsUnique { get; set; }

        #region Allors
        [Id("7cd243af-b334-4456-81f7-b2fe8777bec1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SC1WC1Many2Manies { get; set; }

        #region Allors
        [Id("e6348fc3-4972-4c44-83c6-be50dbb086f0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SC1WC1Many2One { get; set; }

        #region Allors
        [Id("59283310-b978-4f64-8f9b-ec2835ee6e45")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SC1WC1One2Manies { get; set; }

        #region Allors
        [Id("e2cbb55a-10e0-4918-b2c0-0bb1faacc12d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SC1WC1One2One { get; set; }

        #region Allors
        [Id("1407787f-e3a3-493e-bfc2-2bd7eb0fc686")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SC1WC2Many2Manies { get; set; }

        #region Allors
        [Id("e5db203d-1245-47eb-aeac-ccf8985fba54")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SC1WC2Many2One { get; set; }

        #region Allors
        [Id("5e4b79ea-be2c-4177-81f3-6d788488c10d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SC1WC2One2Manies { get; set; }

        #region Allors
        [Id("c78438e2-c085-4959-a5b0-bfec811f46fb")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SC1WC2One2One { get; set; }

        #region Allors
        [Id("b7b62dad-8021-4427-ad83-848d5f16916a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SC1WI12Many2Manies { get; set; }

        #region Allors
        [Id("b4cac28d-ed6f-4c6f-934c-c27ae8831031")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SC1WI12Many2One { get; set; }

        #region Allors
        [Id("c71a53a2-fbfa-4c12-8ee7-565b1ecafb94")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SC1WI12One2Manies { get; set; }

        #region Allors
        [Id("e2732451-760f-41f1-87aa-a483ec12584a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SC1WI12One2One { get; set; }

        #region Allors
        [Id("35b4c471-8814-4294-9c36-30e0072edd48")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SC1WI1Many2Manies { get; set; }

        #region Allors
        [Id("7c87fa96-f621-4fc1-aa71-be351c4ce683")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SC1WI1Many2One { get; set; }

        #region Allors
        [Id("aa3a1fad-8591-4968-98fb-ad2c15de8571")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SC1WI1One2Manies { get; set; }

        #region Allors
        [Id("7d4c62b4-e214-4d23-956a-2238992bb37f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SC1WI1One2One { get; set; }

        #region Allors
        [Id("d569148b-6f7e-417e-b02e-af63401efc29")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2[] SC1WI2Many2Manies { get; set; }

        #region Allors
        [Id("5846cbf8-7e49-47f4-b11a-7d07e3927003")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SC1WI2Many2One { get; set; }

        #region Allors
        [Id("f3e4db8c-ba7d-40cc-9d0d-62a1e86bf738")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] SC1WI2One2Manies { get; set; }

        #region Allors
        [Id("cc9bf91e-8c8a-4328-b82e-b36f5aac4e50")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SC1WI2One2One { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("2e0eb835-e164-4bf4-8be4-d034d297e851")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] DatabaseC1AllorsBinary { get; set; }

        #region Allors
        [Id("d9a1abf5-58ea-4cdb-ab30-bb9f7f13a844")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool DatabaseC1AllorsBoolean { get; set; }

        #region Allors
        [Id("8c189ecf-2c73-46b9-9d56-431b36caf21f")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime DatabaseC1AllorsDateTime { get; set; }

        #region Allors
        [Id("1257f492-9101-438a-aff3-8efcabf8e656")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime DatabaseC1DateTimeLessThan { get; set; }

        #region Allors
        [Id("c5ebad3c-44b2-4a44-8450-f4445bd40479")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime DatabaseC1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("2ccb82e1-a87d-4551-8610-25ebc5aac3cf")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime DatabaseC1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("bc0a2c4a-67a9-4562-94bb-ad99deaebb07")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime DatabaseC1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("714310ba-2f84-4e0f-bcee-dc755a2e3713")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal DatabaseC1AllorsDecimal { get; set; }

        #region Allors
        [Id("d4e00752-ec8a-4af9-a3e8-da428287e1d3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal DatabaseC1DecimalLessThan { get; set; }

        #region Allors
        [Id("3c5b2a7e-9c77-4054-a09e-f42501cc007a")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal DatabaseC1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("f79a2a5c-eb94-498a-9fa0-836ec3ad18fe")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal DatabaseC1DecimalBetweenA { get; set; }

        #region Allors
        [Id("0b8b368b-7658-4c8b-94d8-db73d9feb408")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal DatabaseC1DecimalBetweenB { get; set; }

        #region Allors
        [Id("884b40d8-556d-421c-ae5a-6908c3c17d24")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double DatabaseC1AllorsDouble { get; set; }

        #region Allors
        [Id("24ee1eb5-ba19-4adf-9d43-f3eee9287020")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double DatabaseC1DoubleLessThan { get; set; }

        #region Allors
        [Id("c8040647-9528-4602-b61d-a2816f82450b")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double DatabaseC1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("52826f68-f8f8-418f-8184-eb9a4d476bbd")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double DatabaseC1DoubleBetweenA { get; set; }

        #region Allors
        [Id("cc59bb23-844e-44bd-8a8d-98c5c4c5ab3b")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double DatabaseC1DoubleBetweenB { get; set; }

        #region Allors
        [Id("736a63cd-3267-4fb9-b3d9-52e33ba76200")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int DatabaseC1AllorsInteger { get; set; }

        #region Allors
        [Id("c0a86b07-4bfd-4191-a2b0-867be32d8db0")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int DatabaseC1IntegerLessThan { get; set; }

        #region Allors
        [Id("d493ea1d-a801-4240-a00b-864265fe0aa4")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int DatabaseC1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("a9af270f-cd0e-4ec3-a729-74726bcdde4f")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int DatabaseC1IntegerBetweenA { get; set; }

        #region Allors
        [Id("97500e2f-756d-425e-9fdd-d9e633cb2f1d")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int DatabaseC1IntegerBetweenB { get; set; }

        #region Allors
        [Id("d96d7fa4-11c5-49a7-b90a-6a8b8f58f81c")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string DatabaseC1AllorsString { get; set; }

        #region Allors
        [Id("0e96c964-9faf-4a1c-ac34-5539ee665376")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string DatabaseC1AllorsStringEquals { get; set; }

        #region Allors
        [Id("8045d4db-d4cd-4457-bf53-6139844b556b")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string DatabaseAllorsStringMax { get; set; }

        #region Allors
        [Id("49948892-cbe8-4d64-8d25-dd4fc119aaab")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid DatabaseC1AllorsUnique { get; set; }

        #region Allors
        [Id("70a6b4fe-7bdf-4c88-b1e5-f20e0d207654")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SC1DatabaseC1Many2Manies { get; set; }

        #region Allors
        [Id("759bb1f4-79cf-417a-8ed6-86dcb706fafe")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SC1DatabaseC1Many2One { get; set; }

        #region Allors
        [Id("16153092-2f39-480f-867c-87771a6dc011")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SC1DatabaseC1One2Manies { get; set; }

        #region Allors
        [Id("86f71e85-9fb5-4c40-9c09-e986ce482cf2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SC1DatabaseC1One2One { get; set; }

        #region Allors
        [Id("c9e31f48-8cbc-4c3c-84e1-6e12ca41ff4d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SC1DatabaseC2Many2Manies { get; set; }

        #region Allors
        [Id("8b1e5550-038d-4d51-91d5-48fc8a69e877")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SC1DatabaseC2Many2One { get; set; }

        #region Allors
        [Id("7fc804f2-ce66-4390-bb12-08d055222306")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SC1DatabaseC2One2Manies { get; set; }

        #region Allors
        [Id("96aa7434-f1a8-478b-a225-0e99e2e94031")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SC1DatabaseC2One2One { get; set; }

        #region Allors
        [Id("a2ccdc0c-2f97-4046-9077-7b02c404f8e0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SC1DatabaseI12Many2Manies { get; set; }

        #region Allors
        [Id("f85b7f30-7df1-4af6-9c9c-78f582534a0b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SC1DatabaseI12Many2One { get; set; }

        #region Allors
        [Id("5604c455-5e32-49cb-bf59-c3e0089e5af0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SC1DatabaseI12One2Manies { get; set; }

        #region Allors
        [Id("98bcb1f7-9e7b-4530-ad7c-42142a05030d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SC1DatabaseI12One2One { get; set; }

        #region Allors
        [Id("9d1244dd-54e0-4494-b981-06715c1f31e1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SC1DatabaseI1Many2Manies { get; set; }

        #region Allors
        [Id("14c1f8bd-6a29-4f29-9b4b-023f024d8518")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SC1DatabaseI1Many2One { get; set; }

        #region Allors
        [Id("5b852463-2550-42b1-a0ca-e26023238171")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SC1DatabaseI1One2Manies { get; set; }

        #region Allors
        [Id("a1e0c34e-4838-4682-b4bb-4876e01c5e8f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SC1DatabaseI1One2One { get; set; }

        #region Allors
        [Id("c1e73af0-8a4a-4360-8e56-55c891f4da48")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2[] SC1DatabaseI2Many2Manies { get; set; }

        #region Allors
        [Id("d5cc1f26-10c2-4019-9b2f-c8b77542d899")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SC1DatabaseI2Many2One { get; set; }

        #region Allors
        [Id("14e7f383-d6e5-45f6-813b-15e2833a45f9")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] SC1DatabaseI2One2Manies { get; set; }

        #region Allors
        [Id("d54ceba6-1eaf-47c5-8f5c-0a30877ca8d5")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SC1DatabaseI2One2One { get; set; }

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
