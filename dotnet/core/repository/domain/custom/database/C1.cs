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

        public Restriction[] Restrictions { get; set; }

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

        #region Workspace Relation
        #region Unit Role
        #region Allors
        [Id("15C46CC3-2FC2-4D02-A8D1-CB5670625D93")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public byte[] WorkspaceAllorsBinary { get; set; }

        #region Allors
        [Id("B5B3B3EA-FE60-4CA9-8B5E-C96EC8C41D67")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public bool WorkspaceAllorsBoolean { get; set; }

        #region Allors
        [Id("6F60EFE6-3A64-4E97-8B80-CD0A35FC6FF8")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceAllorsDateTime { get; set; }

        #region Allors
        [Id("600CC7EF-85CD-4256-BF4B-0FA50AEC7C14")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeLessThan { get; set; }

        #region Allors
        [Id("AFD7D95E-45AF-4B23-8713-8E99A16C6CEA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeGreaterThan { get; set; }

        #region Allors
        [Id("E4AA932A-0383-4D55-B60B-DDC5552DE206")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeBetweenA { get; set; }

        #region Allors
        [Id("9071A092-9835-4ECF-9227-4A409AC12A31")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceDateTimeBetweenB { get; set; }

        #region Allors
        [Id("1AF994D3-CC7E-42D3-B0B2-012DADAB89CE")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public decimal WorkspaceAllorsDecimal { get; set; }

        #region Allors
        [Id("CDD87B18-578F-4C8E-8EA0-97569C59D9E9")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalLessThan { get; set; }

        #region Allors
        [Id("22D4BAA1-30DF-4A6C-A50F-CB8B11CC713E")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalGreaterThan { get; set; }

        #region Allors
        [Id("6F3AC57A-F7FF-41E2-90EA-B91ACDB7420B")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalBetweenA { get; set; }

        #region Allors
        [Id("A6C37429-EEDD-41DA-A1C6-D5754D61DDC6")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Decimal WorkspaceDecimalBetweenB { get; set; }

        #region Allors
        [Id("589E6E29-5BFC-49B3-9C61-5F7988FEA1AE")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public double WorkspaceAllorsDouble { get; set; }

        #region Allors
        [Id("1554F2AA-CEB7-4160-AE95-3D31512BB7FE")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleLessThan { get; set; }

        #region Allors
        [Id("947A76F0-FB20-4C72-AFB6-3EE97253E4CB")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleGreaterThan { get; set; }

        #region Allors
        [Id("AF5C1BC3-A863-48D4-B033-7B5B2BD02BAA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleBetweenA { get; set; }

        #region Allors
        [Id("4344B457-1CCB-42CD-AF53-B4564A599B49")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Double WorkspaceDoubleBetweenB { get; set; }

        #region Allors
        [Id("8B0D480A-C417-4E5F-B1DD-8E364AD700EC")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceAllorsInteger { get; set; }

        #region Allors
        [Id("DE965B08-9649-44BD-A3FC-AD81677EB823")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerLessThan { get; set; }

        #region Allors
        [Id("89BE98A0-946C-49DD-8809-040E089ECD5E")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerGreaterThan { get; set; }

        #region Allors
        [Id("C05A1110-2208-4C58-BBC8-8A8278A26234")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerBetweenA { get; set; }

        #region Allors
        [Id("73BC994E-8A33-48B5-AE19-227371029BAB")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceIntegerBetweenB { get; set; }

        #region Allors
        [Id("335F9D62-9E71-4FEB-892D-D71A759A444F")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsString { get; set; }

        #region Allors
        [Id("2682E786-7164-4402-8189-E8706711E326")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsStringEquals { get; set; }

        #region Allors
        [Id("D4F46C1A-B74A-4A9C-BD38-837AC280DB01")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceAllorsStringMax { get; set; }

        #region Allors
        [Id("CF6E50EE-A85D-464E-B3CD-523F7D537A7C")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public Guid WorkspaceAllorsUnique { get; set; }
        #endregion

        #region Workspace Role
        #region Allors
        [Id("C4330979-BA09-4DF6-8644-13EA40638547")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1[] WorkspaceWC1Many2Manies { get; set; }

        #region Allors
        [Id("0FCE9D08-0879-4844-A9FF-B49ABC07F7CA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1 WorkspaceWC1Many2One { get; set; }

        #region Allors
        [Id("D0259FB4-0D77-4C86-9EB6-9C47936182C5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1[] WorkspaceWC1One2Manies { get; set; }

        #region Allors
        [Id("74157BF7-563C-4DD8-8741-8E063DC26377")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC1 WorkspaceWC1One2One { get; set; }

        #region Allors
        [Id("09E1E08B-2F2F-45C3-A89A-27314BAA6BAD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2[] WorkspaceWC2Many2Manies { get; set; }

        #region Allors
        [Id("5225F8F5-5C0A-42BB-8E88-EF1CFBF1DCFE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2 WorkspaceWC2Many2One { get; set; }

        #region Allors
        [Id("4A174265-181C-4B5C-BD27-C626F100B535")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2[] WorkspaceWC2One2Manies { get; set; }

        #region Allors
        [Id("148B8CF5-6316-43C0-B755-B7E5CD5F5B93")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WC2 WorkspaceWC2One2One { get; set; }

        #region Allors
        [Id("72457AA7-BCA9-421C-9C4F-AAF202D56A88")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12[] WorkspaceWI12Many2Manies { get; set; }

        #region Allors
        [Id("C0482F89-F105-45F3-B571-B3B45511C4E2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12 WorkspaceWI12Many2One { get; set; }

        #region Allors
        [Id("7B04EA82-63AC-48DF-8B72-5938E5552AAC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12[] WorkspaceWI12One2Manies { get; set; }

        #region Allors
        [Id("86E4E643-E0BC-4E0C-BE7C-FBA750021C6D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI12 WorkspaceWI12One2One { get; set; }

        #region Allors
        [Id("582AA8F9-B486-4BA6-B45D-231245CE2410")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1[] WorkspaceWI1Many2Manies { get; set; }

        #region Allors
        [Id("84F9FACD-CBC3-49AB-AB55-565878F33910")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1 WorkspaceWI1Many2One { get; set; }

        #region Allors
        [Id("4D2EFCD4-7B63-41C2-A0B5-75661F0E97FD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1[] WorkspaceWI1One2Manies { get; set; }

        #region Allors
        [Id("367157BB-3F63-456E-8489-BE03593624F0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI1 WorkspaceWI1One2One { get; set; }

        #region Allors
        [Id("6F3269D6-8F47-4C24-94EF-072CCF89F084")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2[] WorkspaceWI2Many2Manies { get; set; }

        #region Allors
        [Id("F118D366-352C-4EF7-870D-956F4D8353A5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2 WorkspaceWI2Many2One { get; set; }

        #region Allors
        [Id("A444D88E-6E57-4BA9-891A-BBD06DCCBEB6")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] WorkspaceWI2One2Manies { get; set; }

        #region Allors
        [Id("F61F2B09-8738-46D1-B426-F9E84067741B")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public WI2 WorkspaceWI2One2One { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("CF19A8C8-548B-48A1-A737-9DA6D94F3A8B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("9782A7A4-8864-424A-8560-1BB97623C813")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("09C1A077-8DAD-4E83-A36C-50768904D676")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1[] WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("6351B6CB-11B1-4E80-869D-A914A4548257")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C1 WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("A2FBFF10-D9D5-49C7-AD02-8982FE5181F3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("2BA9D8E6-BCFB-4F1D-9757-C21BB3F6488C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WWorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("AB987C9E-B180-4992-9F9B-BE7C7E3299AC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2[] WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("29A34351-CAE8-4CB6-B853-D63E232E31EC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public C2 WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("4533D90A-CB26-4963-8B58-3AE4C8B55B14")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("7F980D39-FD1B-4341-81E0-8141625CB329")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("187DA18C-C7BA-42BA-A4F1-30DAB6C14409")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12[] WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("9403BBF7-8A33-419D-99C5-31F2896BB040")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I12 WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("649556C7-77A7-4642-9D30-B08AFA087C14")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("5F3B937A-356D-4B25-9EE0-7D7ECE2FB62F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("B3DB6B53-8ED2-4413-9285-5A1DB226D5DD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1[] WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("1E01DE20-32D2-43C9-8407-7A12F32A45C0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I1 WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("02D68748-01B4-4BD8-880F-9BD812E54E5B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2[] WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("83A4C2DA-B8B1-4C90-8116-B2D499A1FA0B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public I2 WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("39CC8AA9-4768-4F68-8AD1-B9C4D730B4A6")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("C9D02346-F36F-4461-87D3-422CDFF0575E")]
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

        #region Session Role
        #region Allors
        [Id("3006EF6B-E689-44ED-A616-650398AFD653")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1Many2Manies { get; set; }

        #region Allors
        [Id("453DD0B5-775C-474D-AE12-9783C6D17CFE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1Many2One { get; set; }

        #region Allors
        [Id("E42BB987-9C4B-4966-9466-A1FE9C0DD2D8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1One2Manies { get; set; }

        #region Allors
        [Id("CC12EC96-AF38-4EB8-8DAC-C5AA9EBAD8F4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1One2One { get; set; }

        #region Allors
        [Id("3B67B7E0-4A0B-4ACA-A1E9-7547651055EF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2Many2Manies { get; set; }

        #region Allors
        [Id("CB6F70DE-6A4C-4D39-97AC-7657A1BC0626")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2Many2One { get; set; }

        #region Allors
        [Id("94ED537D-A39A-4E5A-B8F2-D0AC6AF618D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2One2Manies { get; set; }

        #region Allors
        [Id("EF75424E-510A-4F91-8DF4-B0FCD120A1E1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2One2One { get; set; }

        #region Allors
        [Id("E86F0F48-086B-4378-A071-0B836D20859B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12Many2Manies { get; set; }

        #region Allors
        [Id("D43D2D64-31F0-4F59-A464-385B66300091")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12Many2One { get; set; }

        #region Allors
        [Id("6FE23E34-2A58-48D2-9C89-C2BF7B2E2C9C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12One2Manies { get; set; }

        #region Allors
        [Id("ECF0C419-8AC7-4870-9CBF-AA8D58B85367")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12One2One { get; set; }

        #region Allors
        [Id("CE02036A-0839-4D75-81EC-3C4D2F279AD6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1Many2Manies { get; set; }

        #region Allors
        [Id("927673E5-F25A-4846-9360-7B5FDEC69A13")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1Many2One { get; set; }

        #region Allors
        [Id("886EFC97-B0E8-4BFE-A118-572843465767")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1One2Manies { get; set; }

        #region Allors
        [Id("98E4FC3B-E019-4334-90F0-F03481148605")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1One2One { get; set; }

        #region Allors
        [Id("C1A65AB9-6AB7-4CF4-B941-AC532CAD694C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2[] SessionSI2Many2Manies { get; set; }

        #region Allors
        [Id("5E1199F8-99F8-4919-9F41-33C24CAE1F15")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2Many2One { get; set; }

        #region Allors
        [Id("26D35453-1129-4B31-8238-5008F7822996")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public SI2[] SessionSI2One2Manies { get; set; }

        #region Allors
        [Id("53CAF088-F3FD-4FD8-AA9A-4D54A87E9966")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2One2One { get; set; }
        #endregion

        #region Workspace Role
        #region Allors
        [Id("8805E014-CDB2-4C8F-8E64-64E334C43D9D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1Many2Manies { get; set; }

        #region Allors
        [Id("D906DF27-FCAD-4499-A5FD-AD5CCDBC8A5F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1Many2One { get; set; }

        #region Allors
        [Id("50597991-896E-4F64-8171-D8B711D57450")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1One2Manies { get; set; }

        #region Allors
        [Id("AE4DF0F2-FF93-4890-8C9B-C2B9C0C67EBF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1One2One { get; set; }

        #region Allors
        [Id("0AB545E1-3404-402D-9998-D5EEAC04FB7C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2Many2Manies { get; set; }

        #region Allors
        [Id("A25FA4CF-E548-4B8D-9795-39B66F4FE269")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2Many2One { get; set; }

        #region Allors
        [Id("BAB7148C-2A28-4AFA-B746-7C8FA724099F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2One2Manies { get; set; }

        #region Allors
        [Id("488AA63B-64A3-4926-B2E4-57C08D52708D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2One2One { get; set; }

        #region Allors
        [Id("BA657903-9BDC-4EC1-8F54-4E29EBBC80E3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12Many2Manies { get; set; }

        #region Allors
        [Id("9B708F9E-62A4-4BFA-9363-AF01F942DCC2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12Many2One { get; set; }

        #region Allors
        [Id("0EC33925-870E-439D-8CD0-AD3B6505085D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12One2Manies { get; set; }

        #region Allors
        [Id("C8161C66-D5E4-43E2-AE23-D494757C2E03")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12One2One { get; set; }

        #region Allors
        [Id("8A4934C3-076B-4567-A001-432D312B973F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1Many2Manies { get; set; }

        #region Allors
        [Id("06AAE4AF-29A9-4AED-BBAD-B38BFDA33BBD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1Many2One { get; set; }

        #region Allors
        [Id("C5B083D8-9D0F-44E9-ADB2-6FDEE1F4E909")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1One2Manies { get; set; }

        #region Allors
        [Id("879E484C-5FB4-424B-98A1-B6D38215700E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1One2One { get; set; }

        #region Allors
        [Id("CC4C4256-6CBE-4FC2-A90D-3280C72F6255")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2[] SessionWI2Many2Manies { get; set; }

        #region Allors
        [Id("F305BBA2-4438-4C8E-820D-83B01B098CDB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2Many2One { get; set; }

        #region Allors
        [Id("BCAFF0E9-303C-4823-8F80-1B194F58EE38")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] SessionWI2One2Manies { get; set; }

        #region Allors
        [Id("8FC39E6B-D512-449E-A93F-1EF4B9800B9F")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2One2One { get; set; }
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
