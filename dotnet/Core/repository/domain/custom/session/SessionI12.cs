// <copyright file="I12.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("b240ea82-0031-4b5f-bf30-a2d15deb239e")]
    #endregion
    public partial interface SessionI12 : S12
    {
        #region Allors
        [Id("196c4661-6258-40ea-9900-81d0da1427ae")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] SessionI12AllorsBinary { get; set; }

        #region Allors
        [Id("60cad080-bc67-46d6-9b4f-9e865f94ad40")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int SessionI12AllorsInteger { get; set; }

        #region Allors
        [Id("f2e02969-1612-434b-a6c3-b089e12bffd8")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string SessionI12AllorsString { get; set; }

        #region Allors
        [Id("cf6c1945-34d3-4232-b674-4de8e83e374e")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double SessionI12AllorsDouble { get; set; }

        #region Allors
        [Id("9dde06c0-ed21-4da4-bba7-ce320a34476e")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal SessionI12AllorsDecimal { get; set; }

        #region Allors
        [Id("931d8356-dbcc-4da1-9b09-1e94a939fbc3")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid SessionI12AllorsUnique { get; set; }

        #region Allors
        [Id("7784fb72-4df5-4eb3-a404-39b2f2be5216")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime SessionI12AllorsDateTime { get; set; }

        #region Allors
        [Id("edf7dfda-039f-4824-8b97-38f65b44f07d")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool SessionI12AllorsBoolean { get; set; }

        #region Allors
        [Id("8471aaf4-6c0b-486b-91c4-d3eaefa7f63e")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string SessionName { get; set; }

        #region Allors
        [Id("48bc9e1a-5b63-4e49-bf96-b644447ddca9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI12C1Many2Manies { get; set; }

        #region Allors
        [Id("666cb267-c669-4300-9d1d-a5c93780088e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI12C1One2Manies { get; set; }

        #region Allors
        [Id("5b7da3fc-41ca-46be-9442-7ff3e8289301")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI12C1Many2One { get; set; }

        #region Allors
        [Id("1ba88357-1460-4bcf-ae80-7618781b99f5")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI12C1One2One { get; set; }

        #region Allors
        [Id("6eb50c27-5fb9-493b-a70d-f916b97c3844")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI12C2One2One { get; set; }

        #region Allors
        [Id("f791ef10-1733-4774-a172-bf1d59c61ff4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI12C2Many2Manies { get; set; }

        #region Allors
        [Id("c9535c5d-f171-49e9-a948-f1e825053f54")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI12C2Many2One { get; set; }

        #region Allors
        [Id("b473d477-ac03-4586-a512-d53ce90e1a0f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI12I1Many2One { get; set; }

        #region Allors
        [Id("a95dfa52-c118-42b6-b129-24b58ea69439")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI12I1One2One { get; set; }

        #region Allors
        [Id("81f8568b-2e07-44cf-b452-69447b0b0db1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI12I1One2Manies { get; set; }

        #region Allors
        [Id("fa270183-f6d0-44f6-b92a-1d8181459c2d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI12I1Many2Manies { get; set; }

        #region Allors
        [Id("19cbfeff-bf16-4380-915e-930fcb0713d9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI12I2Many2Manies { get; set; }

        #region Allors
        [Id("9b7ebccb-2184-4844-aeb6-be4cdba36b5f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI12I2Many2One { get; set; }

        #region Allors
        [Id("0bbc5c82-0d61-493a-a6bf-fdc04d077a65")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI12I2One2One { get; set; }

        #region Allors
        [Id("99f0230b-14ed-429d-a3c8-ef7fafbd69d9")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI12I2One2Manies { get; set; }

        #region Allors
        [Id("dfc02521-be67-42e5-aff2-3fe32e395621")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI12I12Many2Manies { get; set; }

        #region Allors
        [Id("7a33c7af-e88f-4f92-8248-78cbe379ec40")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI12I12One2Manies { get; set; }

        #region Allors
        [Id("254a47ea-a20b-4b93-a7af-0f45b92ed240")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI12I12One2One { get; set; }

        #region Allors
        [Id("661c9e17-b056-40bd-bdd1-d134f1ebef6f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionDependencies { get; set; }

        #region Allors
        [Id("c5436dc2-f903-4734-b2d5-74e0545e0d19")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI12I12Many2One { get; set; }
    }
}
