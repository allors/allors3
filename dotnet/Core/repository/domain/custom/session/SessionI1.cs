// <copyright file="I1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("2aaa0ebc-5002-419d-8e5f-b24e2e09ced1")]
    #endregion
    public partial interface SessionI1 : Object, SessionI12, S1
    {
        #region Allors
        [Id("82113f39-2273-45d8-91be-237f3080724b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI1I1Many2One { get; set; }

        #region Allors
        [Id("3ee47288-2f5d-4989-ad9e-f04987dffef5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI1I12Many2Manies { get; set; }

        #region Allors
        [Id("cebd4b4a-aa91-42c6-b002-423a22a725dd")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI1I2Many2Manies { get; set; }

        #region Allors
        [Id("68b31d55-9045-4d69-9acc-049d37c131dd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI1I2Many2One { get; set; }

        #region Allors
        [Id("cc11feac-a88c-4de0-8233-6b024efdf99c")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string SessionI1AllorsString { get; set; }

        #region Allors
        [Id("315740ed-1fd8-46d8-8e61-6ccb6e2d697a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI1I12Many2One { get; set; }

        #region Allors
        [Id("8316eedc-04fe-4a01-a7d4-87ee79b98d4a")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime SessionI1AllorsDateTime { get; set; }

        #region Allors
        [Id("38e82231-e663-4dea-8e76-b4a16387d399")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI1I2One2Manies { get; set; }

        #region Allors
        [Id("cab68c9b-de6b-476f-90a0-dba731c85090")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI1C2One2Manies { get; set; }

        #region Allors
        [Id("5ea7b03c-e156-4730-ba70-2cf2ac3d8c2c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI1C1One2One { get; set; }

        #region Allors
        [Id("05741683-8c13-4986-8d1f-e664e983ab72")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int SessionI1AllorsInteger { get; set; }

        #region Allors
        [Id("c7fed3ce-aa01-4a54-84f6-dc223a513093")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI1C2Many2Manies { get; set; }

        #region Allors
        [Id("ed5dea24-94bd-43f7-b8a3-0915d68d3ca2")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI1I1One2Manies { get; set; }

        #region Allors
        [Id("4f64bf79-5a6f-4837-bd23-e0c06406efa9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI1I1Many2Manies { get; set; }

        #region Allors
        [Id("473f4ac8-8488-4ee2-b07a-f0e4a8ae29bc")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool SessionI1AllorsBoolean { get; set; }

        #region Allors
        [Id("d2ac849f-613d-45af-9d47-e4e4ccc15a66")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal SessionI1AllorsDecimal { get; set; }

        #region Allors
        [Id("e4167110-95f2-43a9-a5ba-c623c2069885")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI1I12One2One { get; set; }

        #region Allors
        [Id("f7e48d3b-4830-4f15-af88-e82022319b3d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI1I2One2One { get; set; }

        #region Allors
        [Id("aa7c893f-3b39-4633-be67-7e023cb38f07")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI1C2One2One { get; set; }

        #region Allors
        [Id("71bda511-e99f-496a-8337-3ddc6f57e8d1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI1C1One2Manies { get; set; }

        #region Allors
        [Id("0a293f7a-4ffe-4824-be58-2f5bb8f356ba")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] SessionI1AllorsBinary { get; set; }

        #region Allors
        [Id("c5053d43-892d-4d2b-a7f5-3a693e14ad31")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI1C1Many2Manies { get; set; }

        #region Allors
        [Id("e216bc5d-18e5-4d0d-a7ab-3502311c6af4")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double SessionI1AllorsDouble { get; set; }

        #region Allors
        [Id("3da433a0-3cdc-4da9-b23a-d3c97ce6c570")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI1I1One2One { get; set; }

        #region Allors
        [Id("ded1febe-b3ea-41d8-956f-6bea87c79dfd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI1C1Many2One { get; set; }

        #region Allors
        [Id("e4be4987-77d8-4f7e-a5f2-b476489729b9")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI1I12One2Manies { get; set; }

        #region Allors
        [Id("bc66dc84-920c-4e8f-86f8-a406773d4b3f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI1C2Many2One { get; set; }

        #region Allors
        [Id("9ac858f8-6f62-4e3c-860a-1494c019cc03")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid SessionI1AllorsUnique { get; set; }

        #region Allors
        [Id("14ca3cf1-a887-4171-8abe-08b8bf7a091e")]
        #endregion
        void InterfaceMethod();
    }
}
