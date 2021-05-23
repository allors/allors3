// <copyright file="C3.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("2a9b5a77-6065-4f2a-bbc3-655426f0f97b")]
    #endregion
    public partial class C3 : System.Object, I3, I23, I34
    {
        #region inherited properties
        public C4[] I3C4many2manies { get; set; }

        public string I3AllorsString { get; set; }

        public I4[] I3I4one2manies { get; set; }

        public C4[] I3C4one2manies { get; set; }

        public I4[] I3I4many2manies { get; set; }

        public I4 I3I4many2one { get; set; }

        public C4 I3C4one2one { get; set; }

        public I4 I3I4one2one { get; set; }

        public C4 I3C4many2one { get; set; }

        public string I3StringEquals { get; set; }

        public C1 C1one2one { get; set; }

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

        public string I23AllorsString { get; set; }

        public decimal I34AllorsDecimal { get; set; }

        public bool I34AllorsBoolean { get; set; }

        public double I34AllorsDouble { get; set; }

        public int I34AllorsInteger { get; set; }

        public string I34AllorsString { get; set; }

        #endregion

        #region Allors
        [Id("02a07b71-a40d-4600-ae12-370be7e973f5")]
        [Size(256)]
        #endregion
        public string C3AllorsString { get; set; }

        #region Allors
        [Id("0e06c403-2a29-4f40-b7b6-3e4fed28aeba")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public C2[] C3C2many2manies { get; set; }

        #region Allors
        [Id("29e76785-f3eb-48b9-a9bf-c44e64762631")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public I4 C3I4one2one { get; set; }

        #region Allors
        [Id("39313684-8ea1-4f15-aada-2a16feb148ea")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C4 C3C4many2one { get; set; }

        #region Allors
        [Id("5e6c2802-3dc5-405a-a2f7-03c9361d4562")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        #endregion
        public C4[] C3C4many2manies { get; set; }

        #region Allors
        [Id("8f2225b7-8c15-414a-a9be-50c757f80b3e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public I4[] C3I4many2manies { get; set; }

        #region Allors
        [Id("92505f70-3611-4ed6-bd27-71030299e176")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C2[] C3C2one2manies { get; set; }

        #region Allors
        [Id("958bc7c6-d609-4407-ba92-50726c9af5d5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public C2 C3C2many2one { get; set; }

        #region Allors
        [Id("b7745909-a63a-448a-b4bd-6caf614c4b12")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        public I4 C3I4many2one { get; set; }

        #region Allors
        [Id("d1601926-ae62-4592-b15b-6511e0d98355")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public C4[] C3C4one2manies { get; set; }

        #region Allors
        [Id("d81da318-f954-42b4-b605-e011a92726ba")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C2 C3C2one2one { get; set; }

        #region Allors
        [Id("da44bf79-b72e-4565-bd33-0eb278a6f4ec")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public C4 C3C4one2one { get; set; }

        #region Allors
        [Id("dd006700-a00c-4c67-819e-1d63df26a5b6")]
        [Size(256)]
        #endregion
        public string C3StringEquals { get; set; }

        #region Allors
        [Id("ed3267fb-fbc4-4e38-87f5-8e2ee91b1bac")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        public I4[] C3I4one2manies { get; set; }

        #region inherited methods
        #endregion

    }
}
