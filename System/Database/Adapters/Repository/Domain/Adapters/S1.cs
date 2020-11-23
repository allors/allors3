// <copyright file="S1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("15c3bb71-075d-48ad-8a00-250c2f627092")]
    #endregion
    public partial interface S1 : Object, S1234
    {
        #region Allors
        [Id("294e7ce3-1b0b-490a-a5e8-6149885d4943")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal S1AllorsDecimal { get; set; }

        #region Allors
        [Id("4cd28d56-ffd6-461c-b9ed-ca0e4bae51df")]
        #endregion
        int S1AllorsInteger { get; set; }

        #region Allors
        [Id("55ab6cfa-651b-48ec-bc33-ad3a381d2260")]
        [Size(-1)]
        #endregion
        byte[] S1AllorsBinary { get; set; }

        #region Allors
        [Id("645c20ac-5b4f-40db-8d11-d2b07123dabe")]
        #endregion
        Guid S1AllorsUnique { get; set; }

        #region Allors
        [Id("678b14c4-b5ae-48e3-ac06-2459cab66c34")]
        [Size(100000)]
        #endregion
        string S1StringLarge { get; set; }

        #region Allors
        [Id("6a166388-5bca-4cd9-bfee-0da27cbc3073")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        S2 S1S2many2one { get; set; }

        #region Allors
        [Id("6ee98698-15dc-4998-88c3-d2a4d1c19e8c")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        S2[] S1S2one2manies { get; set; }

        #region Allors
        [Id("701ca57d-241f-470c-b690-9045c0f76c8f")]
        #endregion
        double S1AllorsDouble { get; set; }

        #region Allors
        [Id("70815e0c-11d4-41ac-b0b2-105f8ede6d27")]
        [Size(256)]
        #endregion
        string S1AllorsString { get; set; }

        #region Allors
        [Id("77afee4a-08b7-4231-aa73-575145efd1e3")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C1 S1C1many2one { get; set; }

        #region Allors
        [Id("8f5485ba-5a82-4d01-809e-52b467f958d8")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C1 S1C1one2one { get; set; }

        #region Allors
        [Id("9fbcf7ce-3b59-458d-ab5e-9c48dd3842b3")]
        #endregion
        bool S1AllorsBoolean { get; set; }

        #region Allors
        [Id("c0cfe3ee-d184-40bd-8354-b0b0bd4e641c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C1[] S1C1many2manies { get; set; }

        #region Allors
        [Id("c6f49460-a259-44de-b674-4d0585fe00cd")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        S2[] S1S2many2manies { get; set; }

        #region Allors
        [Id("dc22175f-185d-4cd3-b492-74b0a9389c91")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        S2 S1S2one2one { get; set; }

        #region Allors
        [Id("e263ac2b-822d-4aa4-8a8c-67db3f2b4bb0")]
        #endregion
        DateTime S1AllorsDateTime { get; set; }

        #region Allors
        [Id("ef918b82-87f4-4591-bf19-2fd5a1019ece")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C1[] S1C1one2manies { get; set; }
    }
}
