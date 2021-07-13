// <copyright file="I2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("d466a7df-1d51-4d38-9ad4-e5e8a4f59a39")]
    #endregion
    public partial interface SessionI2 : Object, SessionI12
    {
        #region Allors
        [Id("24273b38-3fc8-473b-8acd-4969ed58a3dd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI2I2Many2One { get; set; }

        #region Allors
        [Id("61d29099-313b-40b1-ba88-62caa224b82c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI2C1Many2One { get; set; }

        #region Allors
        [Id("ea880241-22b2-4bc4-a5f4-371f0fa9b96f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI2I12Many2One { get; set; }

        #region Allors
        [Id("3cbaa416-4ced-424f-af68-e49de55ef075")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool SessionI2AllorsBoolean { get; set; }

        #region Allors
        [Id("52cbbdde-6eb7-4e83-844f-1cff364b233d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI2C1One2Manies { get; set; }

        #region Allors
        [Id("4ad55fe8-bd51-4b23-a942-a0644cd2d3ee")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI2C1One2One { get; set; }

        #region Allors
        [Id("204a89e7-4a8a-4abe-9f79-374f4626ae56")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal SessionI2AllorsDecimal { get; set; }

        #region Allors
        [Id("5b3cfd66-1a1b-4fba-87de-4c2366b22fe8")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI2I2Many2Manies { get; set; }

        #region Allors
        [Id("51002a0a-31db-41bc-9a73-b9d32c97aaae")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] SessionI2AllorsBinary { get; set; }

        #region Allors
        [Id("3867fc94-6665-4a17-a2f7-4e5481cd7e5c")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid SessionI2AllorsUnique { get; set; }

        #region Allors
        [Id("60be5d07-223f-4b4f-bf43-c13cdad4fb03")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI2I1Many2One { get; set; }

        #region Allors
        [Id("75c43559-19b2-4813-989e-8c35b5ca5d41")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime SessionI2AllorsDateTime { get; set; }

        #region Allors
        [Id("0af71250-77ed-4889-b620-6d83e876931c")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI2I12One2Manies { get; set; }

        #region Allors
        [Id("e470eea2-4524-41fe-9efa-1ee5a7173faa")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI2I12One2One { get; set; }

        #region Allors
        [Id("461db928-4347-4748-9f1f-5fd75236ccd7")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI2C2Many2Manies { get; set; }

        #region Allors
        [Id("8f0da928-54e4-48b0-a6a8-17f1e9b21dfb")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI2I1Many2Manies { get; set; }

        #region Allors
        [Id("6e2cb6db-3047-4cb4-9478-1676ecc7e4a3")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI2C2Many2One { get; set; }

        #region Allors
        [Id("925611c4-cc59-40cd-9a26-7f40b5b2e0f6")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string SessionI2AllorsString { get; set; }

        #region Allors
        [Id("11558191-0308-4016-8a3d-9b58f0968aaf")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI2C2One2Manies { get; set; }

        #region Allors
        [Id("6bf37103-06e8-4648-be66-d675c5c892f1")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI2I1One2One { get; set; }

        #region Allors
        [Id("72585271-8774-4f8a-9069-d7a632c395ea")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI2I1One2Manies { get; set; }

        #region Allors
        [Id("afe7d1a6-ac93-472d-abd7-53d7d892b8cf")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI2I12Many2Manies { get; set; }

        #region Allors
        [Id("95e0e2c0-c37f-4e65-a06c-9b4935edf946")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI2I2One2One { get; set; }

        #region Allors
        [Id("e0ed69e5-46b9-437f-a38a-b1c241e6b3b3")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int SessionI2AllorsInteger { get; set; }

        #region Allors
        [Id("4a3c2123-48e9-4aea-b111-523e2be72956")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI2I2One2Manies { get; set; }

        #region Allors
        [Id("ab624c65-aed4-43dc-b4c1-5b2bfaede713")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI2C1Many2Manies { get; set; }

        #region Allors
        [Id("e14960fd-b6aa-4b57-8bef-fce918665966")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI2C2One2One { get; set; }

        #region Allors
        [Id("c17b39d9-d20e-4b9f-856c-e0b6e2aef2e6")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double SessionI2AllorsDouble { get; set; }
    }
}
