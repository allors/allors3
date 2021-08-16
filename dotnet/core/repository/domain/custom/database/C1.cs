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

        #region Database Origin
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

        #region Workspace Origin
        #region Allors
        [Id("5C87E16B-97AA-4EEB-B995-F8876F7E8CB1")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public byte[] WorkspaceC1AllorsBinary { get; set; }

        #region Allors
        [Id("A133CE98-3C94-47C9-BE6F-3F86ADE525C6")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public bool WorkspaceC1AllorsBoolean { get; set; }

        #region Allors
        [Id("99017A3E-3D2A-4623-9619-B80EE9D7B5FA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public DateTime WorkspaceC1AllorsDateTime { get; set; }
        
        #region Allors
        [Id("A22DFB36-ED53-460F-9875-6A0DE6DF86ED")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public decimal WorkspaceC1AllorsDecimal { get; set; }
        
        #region Allors
        [Id("7342D9DB-8AE2-4F24-ADC0-B06ACF7A6ED5")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public double WorkspaceC1AllorsDouble { get; set; }

        #region Allors
        [Id("E24CD7E6-195F-47A0-BDE6-E165C3FDE007")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public int WorkspaceC1AllorsInteger { get; set; }

        #region Allors
        [Id("4E7CF28C-186C-4C62-B511-C34B81F6F34A")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string WorkspaceC1AllorsString { get; set; }

        #region Allors
        [Id("63C834E9-45C2-4551-92A0-D41E62B338C3")]
        [Origin(Origin.Workspace)]
        #endregion
        [Workspace(Default)]
        public Guid WorkspaceC1AllorsUnique { get; set; }
        #endregion

        #region Session Origin
        #region Allors
        [Id("ACB8ABCB-5187-4A84-B0B5-BC0482527F2F")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SessionC1AllorsBinary { get; set; }

        #region Allors
        [Id("7DAE7CEF-0567-40B9-B8E9-AFF2B888753D")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SessionC1AllorsBoolean { get; set; }

        #region Allors
        [Id("5BC636B3-91E4-44EB-81DB-893FFB6536B8")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionC1AllorsDateTime { get; set; }

        #region Allors
        [Id("8A02BC5B-49FE-46CD-A8FE-C5316DA7221B")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SessionC1AllorsDecimal { get; set; }

        #region Allors
        [Id("A2B28AC8-6DDB-4AA7-BCF2-592F666747FD")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SessionC1AllorsDouble { get; set; }

        #region Allors
        [Id("FB9E631C-667B-40CA-B965-FF40290CCD4B")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionC1AllorsInteger { get; set; }

        #region Allors
        [Id("A3057957-50DB-4A29-9B0E-BB8552EF57A7")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionC1AllorsString { get; set; }

        #region Allors
        [Id("8590B16E-D67D-49DF-98FB-983F44C6210E")]
        [Origin(Origin.Session)]
        #endregion
        [Workspace(Default)]
        public Guid SessionC1AllorsUnique { get; set; }
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
