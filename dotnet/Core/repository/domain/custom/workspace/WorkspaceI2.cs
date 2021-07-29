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
    [Id("e036bbb6-6eb4-48e8-a9f6-6829fb5e2d8d")]
    #endregion
    public partial interface WorkspaceI2 : Object, WorkspaceI12
    {
        #region Workspace

        #region Allors
        [Id("4e92e4ed-8d0f-40db-9d3e-eaaf4eb2d24f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI2 WorkspaceI2WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("8ad6ca13-4f29-40e6-b0b5-6f22163a895f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC1 WorkspaceI2WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("d8d3b1de-c224-4f60-acac-bfd8904e6904")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI12 WorkspaceI2WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("99950f97-d21e-4151-8bbb-e178a1fb06fe")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool WorkspaceI2AllorsBoolean { get; set; }

        #region Allors
        [Id("303f1aed-00f5-4dcf-9ff4-d4248f24c920")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC1[] WorkspaceI2WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("cb9e3395-5b3e-47ba-bb85-1c082028cf9d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC1 WorkspaceI2WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("c30a8d32-6b03-4843-92f2-efab035db55b")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal WorkspaceI2AllorsDecimal { get; set; }

        #region Allors
        [Id("c4e7a99b-5b5f-4e1a-a870-f2c05f116142")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI2[] WorkspaceI2WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("72be4e3f-2c6c-4a12-94a8-24fdff1dd1de")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] WorkspaceI2AllorsBinary { get; set; }

        #region Allors
        [Id("cf71aa5e-738d-42ef-b4f4-cba970e379b9")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid WorkspaceI2AllorsUnique { get; set; }

        #region Allors
        [Id("05c98301-0036-4ed7-af5a-a9eb45485e7e")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI1 WorkspaceI2WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("af92a8c0-5629-4ec0-87ba-ed5e2e627a14")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime WorkspaceI2AllorsDateTime { get; set; }

        #region Allors
        [Id("ec76e1c4-169c-4b28-a115-cdc3fe3c9510")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI12[] WorkspaceI2WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("a3a89baa-2ecc-4400-9b31-fd485e1a8e89")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI12 WorkspaceI2WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("f28aa425-3f91-4099-a4a3-db23c4404230")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC2[] WorkspaceI2WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("4c219ce8-5f80-4987-aebd-c3c1b92a7100")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI1[] WorkspaceI2WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("c115c399-f289-4238-b37e-efc2c31f62d7")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC2 WorkspaceI2WorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("f1f24185-2ba0-467b-8150-3c618740b959")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string WorkspaceI2AllorsString { get; set; }

        #region Allors
        [Id("5001b688-81a5-4814-88cd-5eaf99518c9b")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC2[] WorkspaceI2WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("5cfbd348-0a41-4dde-a98d-fa12cbd5bfad")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI1 WorkspaceI2WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("cb10da3f-bbef-4ac5-b36c-0c0913e3807e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI1[] WorkspaceI2WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("c642f426-057d-4b08-b156-5f1b7f500639")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI12[] WorkspaceI2WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("461f9b62-7a4c-49c4-a95b-c2536e345ae4")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI2 WorkspaceI2WorkspaceI2One2One { get; set; }

        #region Allors
        [Id("7ebc2d95-444a-409f-8475-f6cbb5016355")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int WorkspaceI2AllorsInteger { get; set; }

        #region Allors
        [Id("f2070f54-32b6-42d0-8a18-cd8f2268b322")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceI2[] WorkspaceI2WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("4254cc2c-5fd8-4555-8683-df22ce22719e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC1[] WorkspaceI2WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("fc6a3880-08c1-45ee-ab3d-d6b45a2d9f79")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        WorkspaceC2 WorkspaceI2WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("21c33224-fdde-42ed-a4e5-67a6cdae35a3")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double WorkspaceI2AllorsDouble { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("a22bc0d4-4881-4c69-9c52-67bcd591a148")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI2DatabaseI2Many2One { get; set; }

        #region Allors
        [Id("bee2a0df-54da-4a7e-9186-5e1891b1d0dd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI2DatabaseC1Many2One { get; set; }

        #region Allors
        [Id("fdf400e7-bbde-45bd-9121-41c1ba8227e5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI2DatabaseI12Many2One { get; set; }

        #region Allors
        [Id("4ad2c27b-28ba-4b52-b371-4fe8f630c6cd")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool DatabaseI2AllorsBoolean { get; set; }

        #region Allors
        [Id("806f49d9-46b7-436c-aae5-2f1b312d740d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI2DatabaseC1One2Manies { get; set; }

        #region Allors
        [Id("bf88157c-ec9b-4689-87af-6f950d7106f2")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI2DatabaseC1One2One { get; set; }

        #region Allors
        [Id("955af5e9-b8c6-471c-ac44-3bf97c0ac600")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal DatabaseI2AllorsDecimal { get; set; }

        #region Allors
        [Id("9a976b37-6c1e-482c-a11a-6c918aa0908e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI2DatabaseI2Many2Manies { get; set; }

        #region Allors
        [Id("fde6bb20-0c7a-4a36-8eec-bfd5a073b1c4")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] DatabaseI2AllorsBinary { get; set; }

        #region Allors
        [Id("1404a0d4-12cb-409a-b627-525e0cdc446d")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid DatabaseI2AllorsUnique { get; set; }

        #region Allors
        [Id("b5d69158-790b-4d9b-af06-fe32e0e0f4e7")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI2DatabaseI1Many2One { get; set; }

        #region Allors
        [Id("03368319-2daf-47fd-9483-1479c4934a53")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime DatabaseI2AllorsDateTime { get; set; }

        #region Allors
        [Id("f31f8cb1-a2d0-498a-9c33-8bf1fd8411b6")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI2DatabaseI12One2Manies { get; set; }

        #region Allors
        [Id("c5070983-03a8-47a4-a6df-3da1f05c559c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI2DatabaseI12One2One { get; set; }

        #region Allors
        [Id("fa6831ed-b1bf-495d-a9ae-483c92dfc88c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] WorkspaceI2DatabaseC2Many2Manies { get; set; }

        #region Allors
        [Id("521e1963-64bc-4c09-9d2d-fe88b5bde108")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI2DatabaseI1Many2Manies { get; set; }

        #region Allors
        [Id("012d8f25-ec59-424d-bc7f-aa4bacc65cdc")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI2DatabaseC2Many2One { get; set; }

        #region Allors
        [Id("490b3192-d3f5-4b6e-ae44-8e027abf9200")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string DatabaseI2AllorsString { get; set; }

        #region Allors
        [Id("99ff0a4f-171d-4e18-8e44-1a8e30747fbd")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] WorkspaceI2DatabaseC2One2Manies { get; set; }

        #region Allors
        [Id("930c40bb-05bf-4c5f-9ccb-2bc8d4f81824")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI2DatabaseI1One2One { get; set; }

        #region Allors
        [Id("b9cb431c-f5f2-48a7-b8a5-a2228530fba9")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI2DatabaseI1One2Manies { get; set; }

        #region Allors
        [Id("38b2324a-e96d-4cfd-93f5-f9f6527682f4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI2DatabaseI12Many2Manies { get; set; }

        #region Allors
        [Id("1a79b2dd-1451-4b3b-a402-af5cece5dc4c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI2DatabaseI2One2One { get; set; }

        #region Allors
        [Id("038fd5f9-7c17-4926-89e5-48c398dcb18a")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int DatabaseI2AllorsInteger { get; set; }

        #region Allors
        [Id("868be78d-ec1f-4a49-9910-78667def2652")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI2DatabaseI2One2Manies { get; set; }

        #region Allors
        [Id("67529deb-6f55-4e8f-b334-e60a1b7c978c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI2DatabaseC1Many2Manies { get; set; }

        #region Allors
        [Id("0d1fd667-50fd-44ed-814d-ee6a61a380fe")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI2DatabaseC2One2One { get; set; }

        #region Allors
        [Id("8a9ebfc6-fa6f-4c94-906b-394ff2b2b32b")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double DatabaseI2AllorsDouble { get; set; }

        #endregion

    }
}
