// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("7041c691-d896-4628-8f50-1c24f5d03414")]
    #endregion
    public partial class C1 : System.Object, I1, I12
    {
        #region inherited properties
        public I34[] I1I34one2manies { get; set; }

        public I2[] I1I2one2manies { get; set; }

        public I2 I1I2many2one { get; set; }

        public C2 I1C2many2one { get; set; }

        public C2 I1C2one2one { get; set; }

        public decimal I1DecimalBetweenA { get; set; }

        public S1 I1S1one2one { get; set; }

        public I12 I1I12many2one { get; set; }

        public string I1AllorsString { get; set; }

        public DateTime I1DateTimeLessThan { get; set; }

        public C2[] I1C2one2manies { get; set; }

        public string I1StringLarge { get; set; }

        public double I1FloatLessThan { get; set; }

        public DateTime I1AllorsDateTime { get; set; }

        public C1 I1C1many2one { get; set; }

        public I12 I1I12one2one { get; set; }

        public decimal I1DecimalGreaterThan { get; set; }

        public C1 I1C1one2one { get; set; }

        public I2[] I1I2many2manies { get; set; }

        public int I1IntegerBetweenA { get; set; }

        public I34 I1I34many2one { get; set; }

        public double I1FloatBetweenA { get; set; }

        public int I1IntegerLessThan { get; set; }

        public int I1AllorsInteger { get; set; }

        public S2 I1S2one2one { get; set; }

        public bool I1AllorsBoolean { get; set; }

        public I1 I1I1many2one { get; set; }

        public C1[] I1C1many2manies { get; set; }

        public I2 I1I2one2one { get; set; }

        public decimal I1AllorsDecimal { get; set; }

        public S1[] I1S1many2manies { get; set; }

        public DateTime I1DateTimeGreaterThan { get; set; }

        public I34[] I1I34many2manies { get; set; }

        public I34 I1I34one2one { get; set; }

        public I1[] I1I1one2manies { get; set; }

        public I1[] I1I1many2manies { get; set; }

        public S2[] I1S2many2manies { get; set; }

        public I12[] I1I12many2manies { get; set; }

        public string I1StringEquals { get; set; }

        public I12[] I1I12one2manies { get; set; }

        public S2[] I1S2one2manies { get; set; }

        public C2[] I1C2many2manies { get; set; }

        public byte[] I1AllorsBinary { get; set; }

        public decimal I1DecimalBetweenB { get; set; }

        public double I1FloatGreaterThan { get; set; }

        public int I1IntegerBetweenB { get; set; }

        public DateTime I1DateTimeBetweenA { get; set; }

        public double I1AllorsDouble { get; set; }

        public S1[] I1S1one2manies { get; set; }

        public I1 I1I1one2one { get; set; }

        public int I1IntegerGreaterThan { get; set; }

        public S1 I1S1many2one { get; set; }

        public double I1FloatBetweenB { get; set; }

        public decimal I1DecimalLessThan { get; set; }

        public DateTime I1DateTimeBetweenB { get; set; }

        public Guid I1AllorsUnique { get; set; }

        public C1[] I1C1one2manies { get; set; }

        public S2 I1S2many2one { get; set; }

        public decimal S1AllorsDecimal { get; set; }

        public int S1AllorsInteger { get; set; }

        public byte[] S1AllorsBinary { get; set; }

        public Guid S1AllorsUnique { get; set; }

        public string S1StringLarge { get; set; }

        public S2 S1S2many2one { get; set; }

        public S2[] S1S2one2manies { get; set; }

        public double S1AllorsDouble { get; set; }

        public string S1AllorsString { get; set; }

        public C1 S1C1many2one { get; set; }

        public C1 S1C1one2one { get; set; }

        public bool S1AllorsBoolean { get; set; }

        public C1[] S1C1many2manies { get; set; }

        public S2[] S1S2many2manies { get; set; }

        public S2 S1S2one2one { get; set; }

        public DateTime S1AllorsDateTime { get; set; }

        public C1[] S1C1one2manies { get; set; }

        public string Name { get; set; }

        public double S1234AllorsDouble { get; set; }

        public decimal S1234AllorsDecimal { get; set; }

        public int S1234AllorsInteger { get; set; }

        public S1234 S1234many2one { get; set; }

        public C2 S1234C2one2one { get; set; }

        public C2[] S1234C2many2manies { get; set; }

        public S1234[] S1234one2manies { get; set; }

        public C2[] S1234C2one2manies { get; set; }

        public S1234[] S1234many2manies { get; set; }

        public string ClassName { get; set; }

        public DateTime S1234AllorsDateTime { get; set; }

        public S1234 S1234one2one { get; set; }

        public C2 S1234C2many2one { get; set; }

        public string S1234AllorsString { get; set; }

        public bool S1234AllorsBoolean { get; set; }

        public bool I12AllorsBoolean { get; set; }

        public int I12AllorsInteger { get; set; }

        public I34[] I12I34one2manies { get; set; }

        public C3 C3many2one { get; set; }

        public C2 I12C2many2one { get; set; }

        public double I12AllorsDouble { get; set; }

        public I34 I12I34many2one { get; set; }

        public I34[] I12I34many2manies { get; set; }

        public C3 I12C3one2one { get; set; }

        public C2[] I12C2many2manies { get; set; }

        public decimal I12AllorsDecimal { get; set; }

        public C2 I12C2one2one { get; set; }

        public C3[] I12C3one2manies { get; set; }

        public C3[] I12C3many2manies { get; set; }

        public string PrefetchTest { get; set; }

        public DateTime I12AllorsDateTime { get; set; }

        public string I12AllorsString { get; set; }

        public I34 I12I34one2one { get; set; }

        public C2[] I12C2one2manies { get; set; }

        public string S12AllorsString { get; set; }

        public DateTime S12AllorsDateTime { get; set; }

        public C2[] S12C2many2manies { get; set; }

        public C2 S12C2many2one { get; set; }

        public C2 S12C2one2one { get; set; }

        public C2[] S12C2one2manies { get; set; }

        public bool S12AllorsBoolean { get; set; }

        public double S12AllorsDouble { get; set; }

        public int S12AllorsInteger { get; set; }

        public decimal S12AllorsDecimal { get; set; }

        #endregion

        #region Allors
        [Id("024db9e0-b51f-4d8b-a2d0-0a041dcebd62")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        public decimal C1DecimalBetweenA { get; set; }

        #region Allors
        [Id("03fc18eb-46be-411a-9b1e-4a1953843d92")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public I2 C1I2one2one { get; set; }

        #region Allors
        [Id("0aefa669-9c8a-4fbf-98a4-230d93df8341")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        public decimal C1DecimalBetweenB { get; set; }

        #region Allors
        [Id("0e57dd07-bb58-4620-a898-3060af007f60")]
        [Size(256)]
        #endregion
        public string Argument { get; set; }

        #region Allors
        [Id("10df748e-3b9c-48f4-82dc-85498f199567")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public S1[] C1S1one2manies { get; set; }

        #region Allors
        [Id("13761939-4842-45ba-af73-2a5976e2d6e3")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public I12 C1I12one2one { get; set; }

        #region Allors
        [Id("20713860-8abd-4d71-8ccc-2b4d1b88bce3")]
        [Size(256)]
        #endregion
        public string C1AllorsString { get; set; }

        #region Allors
        [Id("2cd8b843-f1f5-413d-9d6d-0d2b9b3c5cf6")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C1 C1C1many2one { get; set; }

        #region Allors
        [Id("2cee32ad-4e62-4112-9775-f84b0298e93a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public S2 C1S2many2one { get; set; }

        #region Allors
        [Id("2fa10f1e-d7f6-4f75-92a8-15d7b02b8c19")]
        #endregion
        public double C1FloatBetweenA { get; set; }

        #region Allors
        [Id("2fc66f19-7fd4-4dc1-95ef-7931864ad083")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public C1 Many2One { get; set; }

        #region Allors
        [Id("2ff1c9ba-0017-466e-9f11-776086e6d0b0")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        #endregion
        public C1[] C1C1many2manies { get; set; }

        #region Allors
        [Id("3673e4f6-8b40-44e7-be25-d73907b5806a")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public S1[] C1S1many2manies { get; set; }

        #region Allors
        [Id("392e8c95-bbfc-4d24-b751-36c17a7b0ee6")]
        #endregion
        public double C1FloatBetweenB { get; set; }

        #region Allors
        [Id("3fea182f-07b0-4c36-8170-960b484801f6")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public I1 C1I1one2one { get; set; }

        #region Allors
        [Id("49970761-ebe1-4623-a822-5ee1d1f3fafc")]
        #endregion
        public int C1IntegerLessThan { get; set; }

        #region Allors
        [Id("4b970db5-d0ec-4765-9f9b-6e9aafc9dbcc")]
        [Size(100000)]
        #endregion
        public string C1StringLarge { get; set; }

        #region Allors
        [Id("4c0362ad-4d0e-4e57-a057-1852ddd8eed8")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public I2[] C1I2one2manies { get; set; }

        #region Allors
        [Id("4c776502-77d7-45d9-b101-62dee27c0c2e")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C1 C1C1one2one { get; set; }

        #region Allors
        [Id("4c95279f-fb68-49d1-b9c2-27c612c4c28e")]
        #endregion
        public double C1FloatGreaterThan { get; set; }

        #region Allors
        [Id("4dab4e16-b8a2-46c1-949d-62aead9a9c9f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public I2 C1I2many2one { get; set; }

        #region Allors
        [Id("599420c6-0757-49f6-8ae7-4cb0714ca791")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public I12 C1I12many2one { get; set; }

        #region Allors
        [Id("6459deba-24e6-4867-a555-75f672f33893")]
        #endregion
        public DateTime C1DateTimeLessThan { get; set; }

        #region Allors
        [Id("65cff232-60fb-4ed9-9f36-2aebbdc3fc79")]
        [Indexed]
        [Size(-1)]
        #endregion
        public byte[] IndexedMaxBinary { get; set; }

        #region Allors
        [Id("68fa3256-c5ba-42bb-b424-9349f1c6efa3")]
        [Indexed]
        [Size(-1)]
        #endregion
        public string IndexedMaxString { get; set; }

        #region Allors
        [Id("6aadb05d-6b80-47c5-b625-18b86e762c94")]
        #endregion
        public DateTime C1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("71abe169-dea4-4834-8d37-34cbcffa6cee")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public C2[] C1C2many2manies { get; set; }

        #region Allors
        [Id("724f101c-db45-44f3-b9ca-c8f3b0c28d29")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public S1 C1S1many2one { get; set; }

        #region Allors
        [Id("79fbfbc3-50e3-4e45-a5bf-8a253bb6f0c6")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public I1[] C1I1many2manies { get; set; }

        #region Allors
        [Id("7b058b52-dc6b-4f8c-af72-28c9b0c0fde4")]
        #endregion
        public double C1FloatLessThan { get; set; }

        #region Allors
        [Id("7fce490e-78af-46a9-a87d-de233073ab3c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public I1 C1I1many2one { get; set; }

        #region Allors
        [Id("8679b3aa-cdad-4ee1-b4fb-edcefd660edb")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        public decimal C1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("87eb0d19-73a7-4aae-aeed-66dc9163233c")]
        [Precision(10)]
        [Scale(2)]
        #endregion
        public decimal C1AllorsDecimal { get; set; }

        #region Allors
        [Id("92cbd254-9763-41e1-9c73-4a378aab4b8e")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public S2 C1S2one2one { get; set; }

        #region Allors
        [Id("934421bd-6cac-4e99-9457-43117a9f3c52")]
        #endregion
        public DateTime C1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("97f31053-0e7b-42a0-90c2-ce6f09c56e86")]
        [Size(-1)]
        #endregion
        public byte[] C1AllorsBinary { get; set; }

        #region Allors
        [Id("9d8c9863-dd8d-4c85-a5e6-58042ff3619d")]
        #endregion
        public DateTime C1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("9df07ff8-7a29-4d41-a08e-d46efdd15e32")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public S1 C1S1one2one { get; set; }

        #region Allors
        [Id("ab6d11cc-ec86-4828-8875-2e9a779ba627")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C1[] C1C1one2manies { get; set; }

        #region Allors
        [Id("ac0cfbe2-a2ff-4781-83aa-5d4e459d939f")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public I1[] C1I1one2manies { get; set; }

        #region Allors
        [Id("ac2096a9-b58b-41d3-a1d3-920f0b41cb2f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C2 C1C2many2one { get; set; }

        #region Allors
        [Id("ad1b1fb1-b30c-431f-b975-5505f6311a18")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public I12[] C1I12one2manies { get; set; }

        #region Allors
        [Id("b2071550-cc1b-4543-b98f-006e7564a74b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public S2[] C1S2many2manies { get; set; }

        #region Allors
        [Id("b4e3d3d1-65b2-4803-954f-1e09f39e5594")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C2 C1C2one2one { get; set; }

        #region Allors
        [Id("b4ee673f-bba0-4e24-9cda-3cf993c79a0a")]
        #endregion
        public bool C1AllorsBoolean { get; set; }

        #region Allors
        [Id("c58903fb-443b-4de9-b010-15f3f09ff5df")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public I12[] C1I12many2manies { get; set; }

        #region Allors
        [Id("c92fbc53-ae5e-450e-9681-ca17833e6e2f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public I2[] C1I2many2manies { get; set; }

        #region Allors
        [Id("cef13620-b7d7-4bfe-8d3b-c0f826da5989")]
        #endregion
        public Guid C1AllorsUnique { get; set; }

        #region Allors
        [Id("d3f73a6d-8f95-44c6-bbc8-ddc468b803f7")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C3 C1C3one2one { get; set; }

        #region Allors
        [Id("da4d6a24-6b0f-4841-b355-80ee1ba10c59")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public C3[] C1C3many2manies { get; set; }

        #region Allors
        [Id("dc55a574-5546-4a68-b886-706c39bc4e80")]
        [Size(256)]
        #endregion
        public string C1StringEquals { get; set; }

        #region Allors
        [Id("e2153298-73b0-4f5f-bba0-00c832b044b3")]
        #endregion
        public int C1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("e3af3413-4631-4052-ac57-955651a319fc")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C3 C3may2one { get; set; }

        #region Allors
        [Id("e3dedb1d-6738-46f7-8a25-77213c90a8f9")]
        #endregion
        public int C1IntegerBetweenB { get; set; }

        #region Allors
        [Id("ef75cc4e-8787-4f1c-ae5c-73577d721467")]
        #endregion
        public DateTime C1AllorsDateTime { get; set; }

        #region Allors
        [Id("ef909fec-7a03-4a3c-a3f4-6097a51ff1f0")]
        #endregion
        public int C1IntegerBetweenA { get; set; }

        #region Allors
        [Id("f268783d-42ed-41c1-b0b0-b8a60e30a601")]
        #endregion
        public double C1AllorsDouble { get; set; }

        #region Allors
        [Id("f39739d2-e8fc-406e-be6a-c92acee07686")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C2[] C1C2one2manies { get; set; }

        #region Allors
        [Id("f47b9392-1391-416e-9a49-23ab0627133e")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public S2[] C1S2one2manies { get; set; }

        #region Allors
        [Id("f4920d94-8cd0-45b6-be00-f18d377368fd")]
        [Indexed]
        #endregion
        public int C1AllorsInteger { get; set; }

        #region Allors
        [Id("fc56ca04-9737-4b51-939e-4854e5507953")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        public decimal C1DecimalLessThan { get; set; }

        #region Allors
        [Id("fee2d1a8-bb65-4bfe-b25f-407c629dec18")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C3[] C1C3one2manies { get; set; }

        #region inherited methods
        #endregion

    }
}
