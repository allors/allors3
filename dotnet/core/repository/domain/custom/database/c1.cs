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
    public partial class C1 : I1, DerivationCounted, Object
    {
        #region inherited properties
        public I1 I1I1Many2One { get; set; }

        public I12[] I1I12Many2Manies { get; set; }

        public I2[] I1I2Many2Manies { get; set; }

        public I2 I1I2Many2One { get; set; }

        public string I1AllorsString { get; set; }

        public I12 I1I12Many2One { get; set; }

        public DateTime I1AllorsDateTime { get; set; }

        public I2[] I1I2One2Manies { get; set; }

        public C2[] I1C2One2Manies { get; set; }

        public C1 I1C1One2One { get; set; }

        public int I1AllorsInteger { get; set; }

        public C2[] I1C2Many2Manies { get; set; }

        public I1[] I1I1One2Manies { get; set; }

        public I1[] I1I1Many2Manies { get; set; }

        public bool I1AllorsBoolean { get; set; }

        public decimal I1AllorsDecimal { get; set; }

        public I12 I1I12One2One { get; set; }

        public I2 I1I2One2One { get; set; }

        public C2 I1C2One2One { get; set; }

        public C1[] I1C1One2Manies { get; set; }

        public byte[] I1AllorsBinary { get; set; }

        public C1[] I1C1Many2Manies { get; set; }

        public double I1AllorsDouble { get; set; }

        public I1 I1I1One2One { get; set; }

        public C1 I1C1Many2One { get; set; }

        public I12[] I1I12One2Manies { get; set; }

        public C2 I1C2Many2One { get; set; }

        public Guid I1AllorsUnique { get; set; }

        public byte[] I12AllorsBinary { get; set; }

        public C2 I12C2One2One { get; set; }

        public double I12AllorsDouble { get; set; }

        public I1 I12I1Many2One { get; set; }

        public string I12AllorsString { get; set; }

        public I12[] I12I12Many2Manies { get; set; }

        public decimal I12AllorsDecimal { get; set; }

        public I2[] I12I2Many2Manies { get; set; }

        public C2[] I12C2Many2Manies { get; set; }

        public I1[] I12I1Many2Manies { get; set; }

        public I12[] I12I12One2Manies { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public C1[] I12C1Many2Manies { get; set; }

        public I2 I12I2Many2One { get; set; }

        public Guid I12AllorsUnique { get; set; }

        public int I12AllorsInteger { get; set; }

        public I1[] I12I1One2Manies { get; set; }

        public C1 I12C1One2One { get; set; }

        public I12 I12I12One2One { get; set; }

        public I2 I12I2One2One { get; set; }

        public I12[] Dependencies { get; set; }

        public I2[] I12I2One2Manies { get; set; }

        public C2 I12C2Many2One { get; set; }

        public I12 I12I12Many2One { get; set; }

        public bool I12AllorsBoolean { get; set; }

        public I1 I12I1One2One { get; set; }

        public C1[] I12C1One2Manies { get; set; }

        public C1 I12C1Many2One { get; set; }

        public DateTime I12AllorsDateTime { get; set; }

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

        #region Database Relation
        #region Unit
        #region Allors
        [Id("97f31053-0e7b-42a0-90c2-ce6f09c56e86")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public byte[] C1AllorsBinary { get; set; }

        #region Allors
        [Id("b4ee673f-bba0-4e24-9cda-3cf993c79a0a")]
        #endregion
        [Workspace(Default)]
        public bool C1AllorsBoolean { get; set; }

        #region Allors
        [Id("ef75cc4e-8787-4f1c-ae5c-73577d721467")]
        #endregion
        [Workspace(Default)]
        public DateTime C1AllorsDateTime { get; set; }

        #region Allors
        [Id("2170609C-5C25-4F36-935C-96EF49430F05")]
        #endregion
        [Workspace(Default)]
        public DateTime C1DateTimeLessThan { get; set; }

        #region Allors
        [Id("0A86A641-B3AD-44B6-9CFF-BD3DA0DAF087")]
        #endregion
        [Workspace(Default)]
        public DateTime C1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("D5995DE4-87E0-41C0-99AB-1D66765AF3AC")]
        #endregion
        [Workspace(Default)]
        public DateTime C1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("765CCB7F-BA6D-492C-AEDD-458840713EE1")]
        #endregion
        [Workspace(Default)]
        public DateTime C1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("87eb0d19-73a7-4aae-aeed-66dc9163233c")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal C1AllorsDecimal { get; set; }

        #region Allors
        [Id("DF55DC2C-5BF2-4924-AC76-F7EEB958D5EF")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public Decimal C1DecimalLessThan { get; set; }

        #region Allors
        [Id("E3DC625A-7195-4B6F-8CF5-308D158467C3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public Decimal C1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("46F4E42F-DA13-450F-A8AA-8B84356F0345")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public Decimal C1DecimalBetweenA { get; set; }

        #region Allors
        [Id("85DCD714-6D63-46F2-A3D7-88A539743BE6")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public Decimal C1DecimalBetweenB { get; set; }

        #region Allors
        [Id("f268783d-42ed-41c1-b0b0-b8a60e30a601")]
        #endregion
        [Workspace(Default)]
        public double C1AllorsDouble { get; set; }

        #region Allors
        [Id("D0B775C5-4ABE-4DB6-B5E3-14CA6B807CDA")]
        #endregion
        [Workspace(Default)]
        public Double C1DoubleLessThan { get; set; }

        #region Allors
        [Id("FCE30AC0-9283-423A-9CB6-D9D6BAE2FDB7")]
        #endregion
        [Workspace(Default)]
        public Double C1DoubleGreaterThan { get; set; }

        #region Allors
        [Id("8C0CDDDD-00C5-4607-B22E-1870199CDE04")]
        #endregion
        [Workspace(Default)]
        public Double C1DoubleBetweenA { get; set; }

        #region Allors
        [Id("D01DF473-ED93-4B6E-96AA-16313F4EAB32")]
        #endregion
        [Workspace(Default)]
        public Double C1DoubleBetweenB { get; set; }

        #region Allors
        [Id("f4920d94-8cd0-45b6-be00-f18d377368fd")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public int C1AllorsInteger { get; set; }

        #region Allors
        [Id("9B22B6F0-7473-43C7-976C-2817AFE69C29")]
        #endregion
        [Workspace(Default)]
        public int C1IntegerLessThan { get; set; }

        #region Allors
        [Id("48E2F6A9-5441-4587-9EC2-D1F2395C753B")]
        #endregion
        [Workspace(Default)]
        public int C1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("9C0B2263-B74F-44C9-8A55-C54612F26472")]
        #endregion
        [Workspace(Default)]
        public int C1IntegerBetweenA { get; set; }

        #region Allors
        [Id("DE8AC931-A1AC-4FE3-841A-9AFC01752996")]
        #endregion
        [Workspace(Default)]
        public int C1IntegerBetweenB { get; set; }

        #region Allors
        [Id("20713860-8abd-4d71-8ccc-2b4d1b88bce3")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string C1AllorsString { get; set; }

        #region Allors
        [Id("8A1085F6-D8BE-458B-ACF9-E337A15A5268")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string C1AllorsStringEquals { get; set; }

        #region Allors
        [Id("a64abd21-dadf-483d-9499-d19aa8e33791")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string AllorsStringMax { get; set; }

        #region Allors
        [Id("cef13620-b7d7-4bfe-8d3b-c0f826da5989")]
        #endregion
        [Workspace(Default)]
        public Guid C1AllorsUnique { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("8c198447-e943-4f5a-b749-9534b181c664")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public C1[] C1C1Many2Manies { get; set; }

        #region Allors
        [Id("a8e18ea7-cbf2-4ea7-ae14-9f4bcfdb55de")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public C1 C1C1Many2One { get; set; }

        #region Allors
        [Id("a0ac5a65-2cbd-4c51-9417-b10150bc5699")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public C1[] C1C1One2Manies { get; set; }

        #region Allors
        [Id("79c00218-bb4f-40e9-af7d-61af444a4a54")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public C1 C1C1One2One { get; set; }

        #region Allors
        [Id("f29d4a52-9ba5-40f6-ba99-050cbd03e554")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public C2[] C1C2Many2Manies { get; set; }

        #region Allors
        [Id("5490dc63-a8f6-4a86-91ef-fef97a86f119")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public C2 C1C2Many2One { get; set; }

        #region Allors
        [Id("9f6538c2-e6dd-4c27-80ed-2748f645cb95")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public C2[] C1C2One2Manies { get; set; }

        #region Allors
        [Id("e97fc754-c736-4359-9662-19dce9429f89")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public C2 C1C2One2One { get; set; }

        #region Allors
        [Id("94a2b37d-9431-4496-b992-630cda5b9851")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public I12[] C1I12Many2Manies { get; set; }

        #region Allors
        [Id("bcf4df45-6616-4cdf-8ada-f944f9c7ff1a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public I12 C1I12Many2One { get; set; }

        #region Allors
        [Id("98c5f58b-1777-4d9a-8828-37dbf7051510")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public I12[] C1I12One2Manies { get; set; }

        #region Allors
        [Id("b9f2c4c7-6979-40cf-82a2-fa99a5d9e9a4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public I12 C1I12One2One { get; set; }

        #region Allors
        [Id("815878f6-16f2-42f2-9b24-f394ddf789c2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public I1[] C1I1Many2Manies { get; set; }

        #region Allors
        [Id("7bb216f2-8e9c-4dcd-890b-579130ab0a8b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public I1 C1I1Many2One { get; set; }

        #region Allors
        [Id("e0656d9a-75a6-4e59-aaa1-3ff03d440059")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public I1[] C1I1One2Manies { get; set; }

        #region Allors
        [Id("0e7f529b-bc91-4a40-a7e7-a17341c6bf5b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public I1 C1I1One2One { get; set; }

        #region Allors
        [Id("cda97972-84c8-48e3-99d8-fd7c99c5dbc9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public I2[] C1I2Many2Manies { get; set; }

        #region Allors
        [Id("d0341bed-2732-4bcb-b1bb-9f9589de5d03")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public I2 C1I2Many2One { get; set; }

        #region Allors
        [Id("82f5fb26-c260-41bc-a784-a2d5e35243bd")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] C1I2One2Manies { get; set; }

        #region Allors
        [Id("6def7988-4bcf-4964-9de6-c6ede41d5e5a")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public I2 C1I2One2One { get; set; }
        #endregion
        #endregion

        #region Session Relation
        #region Unit Role
        #region Allors
        [Id("9EBD97E5-3A3D-495E-AF74-33350C9F75B2")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SessionAllorsBinary { get; set; }

        #region Allors
        [Id("4B57ACC4-83E6-407F-813F-274C9A8D89AE")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SessionAllorsBoolean { get; set; }

        #region Allors
        [Id("882F70CA-DC24-48A9-9C29-35667BA36FEA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionAllorsDateTime { get; set; }

        #region Allors
        [Id("9ED36932-D572-48CC-8C6D-1160ED9C9238")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeLessThan { get; set; }

        #region Allors
        [Id("BEE45F77-C950-4CC2-A524-967565037F66")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeGreaterThan { get; set; }

        #region Allors
        [Id("E047539A-DF5A-4BAA-9DEC-B4BA3DCE617E")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenA { get; set; }

        #region Allors
        [Id("49D54F4C-E84D-4518-9315-E9D9F6A26CB3")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenB { get; set; }

        #region Allors
        [Id("6DFE4C26-8221-46B0-9B97-27A836503659")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SessionAllorsDecimal { get; set; }

        #region Allors
        [Id("2C1B2B6C-1BE1-47A4-9306-6B131E8810FC")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalLessThan { get; set; }

        #region Allors
        [Id("0D4B3714-3A1C-433C-AC46-017D9D536BC3")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalGreaterThan { get; set; }

        #region Allors
        [Id("7ED95C6D-6CA4-4236-A601-8442106EEEC4")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenA { get; set; }

        #region Allors
        [Id("88A043EB-D377-484A-AF91-C968848ABBBC")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenB { get; set; }

        #region Allors
        [Id("6C0C1D05-0B90-428B-8DB1-0ACB2B2550D9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SessionAllorsDouble { get; set; }

        #region Allors
        [Id("1782D8BC-33F5-401F-A08F-58105C03DB13")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleLessThan { get; set; }

        #region Allors
        [Id("F40D0414-75F5-480D-A613-E663EAE16B16")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleGreaterThan { get; set; }

        #region Allors
        [Id("C99AAE0C-3901-49E1-97BC-8C4E89E6ABE9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenA { get; set; }

        #region Allors
        [Id("4BF9539B-7BB1-4644-8D08-04A8FDFFF8AF")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenB { get; set; }

        #region Allors
        [Id("12CC3460-58B3-4512-8958-19A09937291E")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionAllorsInteger { get; set; }

        #region Allors
        [Id("EE5CAFC8-32A5-467A-99D0-626DDDF9805F")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerLessThan { get; set; }

        #region Allors
        [Id("67817338-5601-40B4-865F-E69A39F9CA85")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerGreaterThan { get; set; }

        #region Allors
        [Id("9D763824-E49E-497D-B73B-344C3DEC0EB0")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenA { get; set; }

        #region Allors
        [Id("B7AE5D69-06AC-4557-8B6F-E2AD08B3E33D")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenB { get; set; }

        #region Allors
        [Id("B31873DC-E720-4143-9CB5-8726B82FE73B")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsString { get; set; }

        #region Allors
        [Id("633A0A0B-E3F0-4780-B120-B52173FD6598")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringEquals { get; set; }

        #region Allors
        [Id("6256F25E-2A6A-4913-A534-ED095CD23B28")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringMax { get; set; }

        #region Allors
        [Id("1BA8356F-9F85-4D1E-AFE9-33BDC21ADA22")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid SessionAllorsUnique { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("77B4D50E-093A-4FFF-8481-4EB088A722A8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1Many2Manies { get; set; }

        #region Allors
        [Id("4BA89471-767B-4FEF-A038-5392040252B8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1Many2One { get; set; }

        #region Allors
        [Id("05547BE9-1E41-48D9-B3AD-30D9E1EF2277")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1One2Manies { get; set; }

        #region Allors
        [Id("E2E727FB-F51D-461D-BCE8-BE0CF4EEB962")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1One2One { get; set; }

        #region Allors
        [Id("F119F7A4-1A8A-4E40-A1CE-7FE829043D20")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("D7F8A5AF-FC82-47BD-B7AB-E995F5A3C8B4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 WSessionC2Many2One { get; set; }

        #region Allors
        [Id("D2377761-4F39-4C93-9DF4-6ED4DEDBCA39")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2One2Manies { get; set; }

        #region Allors
        [Id("2AF4F53B-A17A-47C2-9030-E89789F4F831")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SessionC2One2One { get; set; }

        #region Allors
        [Id("9B3BE90E-39C1-404A-97B3-B11E9D859EF0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("8AE981BB-66D9-48AF-B945-DFEACA7F1B15")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12Many2One { get; set; }

        #region Allors
        [Id("4405DE90-89FD-40DC-97A9-92E32AA7FB3C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12One2Manies { get; set; }

        #region Allors
        [Id("FFB26F6C-CC41-431F-9A8E-69C3FB217AE4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12One2One { get; set; }

        #region Allors
        [Id("70504B1B-7B6C-4379-AAFF-F81727F403AA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1Many2Manies { get; set; }

        #region Allors
        [Id("DC3885D9-6A5E-45AB-9791-5524FF61ABBE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1Many2One { get; set; }

        #region Allors
        [Id("B7E394E0-D217-4A7F-AF21-0471DE2BABFF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1One2Manies { get; set; }

        #region Allors
        [Id("D44BE790-E5A3-47E7-B3D7-EF7C18729518")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1One2One { get; set; }

        #region Allors
        [Id("2B138094-5F16-430A-96E4-C1C720436AE5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2[] SessionI2Many2Manies { get; set; }

        #region Allors
        [Id("AF2865E4-E838-4DA0-9D81-BB573AC9D00D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2Many2One { get; set; }

        #region Allors
        [Id("3F14D81B-6F8C-49E5-A623-1F571CCD67E8")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] SessionI2One2Manies { get; set; }

        #region Allors
        [Id("186CC1D1-FAFC-45A0-99A3-06F95746DAE3")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2One2One { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("09A6A387-A1B5-4038-B074-3A01C81CBDA2")]
        #endregion
        [Workspace(Default)]
        public void ClassMethod() { }

        #region Allors
        [Id("26FE4FD7-68C3-4DDA-8A44-87857B35B000")]
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
