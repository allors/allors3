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

        #endregion

        #region Allors
        [Id("97f31053-0e7b-42a0-90c2-ce6f09c56e86")]
        [Size(-1)]
        [Workspace(Default)]
        #endregion
        public byte[] C1AllorsBinary { get; set; }

        #region Allors
        [Id("b4ee673f-bba0-4e24-9cda-3cf993c79a0a")]
        [Workspace(Default)]
        #endregion
        public bool C1AllorsBoolean { get; set; }

        #region Allors
        [Id("ef75cc4e-8787-4f1c-ae5c-73577d721467")]
        [Workspace(Default)]
        #endregion
        public DateTime C1AllorsDateTime { get; set; }

        #region Allors
        [Id("87eb0d19-73a7-4aae-aeed-66dc9163233c")]

        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        #endregion
        public decimal C1AllorsDecimal { get; set; }

        #region Allors
        [Id("f268783d-42ed-41c1-b0b0-b8a60e30a601")]
        [Workspace(Default)]
        #endregion
        public double C1AllorsDouble { get; set; }

        #region Allors
        [Id("f4920d94-8cd0-45b6-be00-f18d377368fd")]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public int C1AllorsInteger { get; set; }

        #region Allors
        [Id("20713860-8abd-4d71-8ccc-2b4d1b88bce3")]
        [Size(256)]
        [Workspace(Default)]
        #endregion
        public string C1AllorsString { get; set; }

        #region Allors
        [Id("a64abd21-dadf-483d-9499-d19aa8e33791")]
        [Size(-1)]
        [Workspace(Default)]
        #endregion
        public string AllorsStringMax { get; set; }

        #region Allors
        [Id("cef13620-b7d7-4bfe-8d3b-c0f826da5989")]
        [Workspace(Default)]
        #endregion
        public Guid C1AllorsUnique { get; set; }

        #region Allors
        [Id("8c198447-e943-4f5a-b749-9534b181c664")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C1[] C1C1Many2Manies { get; set; }

        #region Allors
        [Id("a8e18ea7-cbf2-4ea7-ae14-9f4bcfdb55de")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C1 C1C1Many2One { get; set; }

        #region Allors
        [Id("a0ac5a65-2cbd-4c51-9417-b10150bc5699")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C1[] C1C1One2Manies { get; set; }

        #region Allors
        [Id("79c00218-bb4f-40e9-af7d-61af444a4a54")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C1 C1C1One2One { get; set; }

        #region Allors
        [Id("f29d4a52-9ba5-40f6-ba99-050cbd03e554")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C2[] C1C2Many2Manies { get; set; }

        #region Allors
        [Id("5490dc63-a8f6-4a86-91ef-fef97a86f119")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C2 C1C2Many2One { get; set; }

        #region Allors
        [Id("9f6538c2-e6dd-4c27-80ed-2748f645cb95")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C2[] C1C2One2Manies { get; set; }

        #region Allors
        [Id("e97fc754-c736-4359-9662-19dce9429f89")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public C2 C1C2One2One { get; set; }

        #region Allors
        [Id("94a2b37d-9431-4496-b992-630cda5b9851")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I12[] C1I12Many2Manies { get; set; }

        #region Allors
        [Id("bcf4df45-6616-4cdf-8ada-f944f9c7ff1a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I12 C1I12Many2One { get; set; }

        #region Allors
        [Id("98c5f58b-1777-4d9a-8828-37dbf7051510")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I12[] C1I12One2Manies { get; set; }

        #region Allors
        [Id("b9f2c4c7-6979-40cf-82a2-fa99a5d9e9a4")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I12 C1I12One2One { get; set; }

        #region Allors
        [Id("815878f6-16f2-42f2-9b24-f394ddf789c2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I1[] C1I1Many2Manies { get; set; }

        #region Allors
        [Id("7bb216f2-8e9c-4dcd-890b-579130ab0a8b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I1 C1I1Many2One { get; set; }

        #region Allors
        [Id("e0656d9a-75a6-4e59-aaa1-3ff03d440059")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I1[] C1I1One2Manies { get; set; }

        #region Allors
        [Id("0e7f529b-bc91-4a40-a7e7-a17341c6bf5b")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I1 C1I1One2One { get; set; }

        #region Allors
        [Id("cda97972-84c8-48e3-99d8-fd7c99c5dbc9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I2[] C1I2Many2Manies { get; set; }

        #region Allors
        [Id("d0341bed-2732-4bcb-b1bb-9f9589de5d03")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I2 C1I2Many2One { get; set; }

        #region Allors
        [Id("82f5fb26-c260-41bc-a784-a2d5e35243bd")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I2[] C1I2One2Manies { get; set; }

        #region Allors
        [Id("6def7988-4bcf-4964-9de6-c6ede41d5e5a")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        public I2 C1I2One2One { get; set; }

        #region Allors
        [Id("09A6A387-A1B5-4038-B074-3A01C81CBDA2")]
        [Workspace(Default)]
        #endregion
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
