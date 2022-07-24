// <copyright file="C2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("72c07e8a-03f5-4da8-ab37-236333d4f74e")]
    #endregion
    public partial class C2 : System.Object, I2, I23, I12
    {
        #region inherited properties
        public bool I2AllorsBoolean { get; set; }

        public decimal I2AllorsDecimal { get; set; }

        public DateTime I2AllorsDateTime { get; set; }

        public string I2AllorsString { get; set; }

        public int I2AllorsInteger { get; set; }

        public double I2AllorsDouble { get; set; }

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

        public string S2AllorsString { get; set; }

        public int S2AllorsInteger { get; set; }

        public double S2AllorsDouble { get; set; }

        public bool S2AllorsBoolean { get; set; }

        public decimal S2AllorsDecimal { get; set; }

        public DateTime S2AllorsDateTime { get; set; }

        public string I23AllorsString { get; set; }

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
        [Id("07eaa992-322a-40e9-bf2c-aa33b69f54cd")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        public decimal C2AllorsDecimal { get; set; }

        #region Allors
        [Id("0947eb06-5511-475f-8d68-06cfb812678e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public C1[] C1many2manies { get; set; }

        #region Allors
        [Id("0ecc2d3b-f813-44db-b349-3e67d7e0b2d7")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C2 C2C2many2one { get; set; }

        #region Allors
        [Id("262ad367-a52c-4d8b-94e2-b477bb098423")]
        #endregion
        public double C2AllorsDouble { get; set; }

        #region Allors
        [Id("42f9f4b6-3b35-4168-93cb-35171dbf83f4")]
        #endregion
        public int C2AllorsInteger { get; set; }

        #region Allors
        [Id("49d04b6f-6393-49f6-bb6b-2dd634d6b9ee")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public C2[] C2C2many2manies { get; set; }

        #region Allors
        [Id("61daaaae-dd22-405e-aa98-6321d2f8af04")]
        #endregion
        public bool C2AllorsBoolean { get; set; }

        #region Allors
        [Id("7ee9d97c-8ae3-438c-adfd-6a35b3ff645b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C1 C1many2one { get; set; }

        #region Allors
        [Id("9540e8d3-9fe3-4aea-9918-fc31210f2622")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C1 C1one2one { get; set; }

        #region Allors
        [Id("9c7cde3f-9b61-4c79-a5d7-afe1067262ce")]
        [Size(256)]
        #endregion
        public string C2AllorsString { get; set; }

        #region Allors
        [Id("9e9d1c6a-f647-4922-b5f4-874b8b6c1907")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C2 C2C2one2one { get; set; }

        #region Allors
        [Id("a95948a7-3f12-4b85-8823-82dea87740c0")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C2[] C2C2one2manies { get; set; }

        #region Allors
        [Id("ce23482d-3a22-4202-98e7-5934fd9abd2d")]
        #endregion
        public DateTime C2AllorsDateTime { get; set; }

        #region Allors
        [Id("d82be8f5-673a-466b-8abb-077be0bc6eb5")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C1[] C1one2manies { get; set; }

        #region Allors
        [Id("d92643c0-854c-40f8-92c8-93a0245e33c2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        #endregion
        public C3[] C3Many2Manies { get; set; }

        #region Allors
        [Id("f3482f88-4408-4e2e-b179-7f757bf0eb3d")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public C3 C3Many2One { get; set; }

        #region inherited methods
        #endregion

    }
}
