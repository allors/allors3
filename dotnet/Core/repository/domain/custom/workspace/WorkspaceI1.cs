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
    [Id("c5cad201-dca3-4615-a789-c3ff0a5beb32")]
    #endregion
    public partial interface WorkspaceI1 : Object, WorkspaceI12, S1
    {
        #region Workspace

        #region Allors
        [Id("d43d79be-2ace-4530-a941-f0298d12b4da")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI1I1Many2One { get; set; }

        #region Allors
        [Id("b57716c5-fa28-4749-ab4f-5a574ea5f871")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI1I12Many2Manies { get; set; }

        #region Allors
        [Id("2cb1e1e4-1026-4170-bf0a-1259b361753b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI1I2Many2Manies { get; set; }

        #region Allors
        [Id("a48a24e2-e2d4-48b3-a090-ecc066f87dcf")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI1I2Many2One { get; set; }

        #region Allors
        [Id("e3a90637-ac66-4162-a0e8-cd2ae7c4edc2")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string WorkspaceI1AllorsString { get; set; }

        #region Allors
        [Id("f81d9bf5-4369-40e2-bcfc-a196817ac5db")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI1I12Many2One { get; set; }

        #region Allors
        [Id("05edfed0-6026-492b-b664-af9f47b8c9ba")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime WorkspaceI1AllorsDateTime { get; set; }

        #region Allors
        [Id("2b1d5f7e-bb16-4549-94db-bcaed2a01e41")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI1I2One2Manies { get; set; }

        #region Allors
        [Id("1e902f93-2b3e-47a1-aa2e-ce85407ff266")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] WorkspaceI1C2One2Manies { get; set; }

        #region Allors
        [Id("d6fa7f5d-10a8-467a-8fab-ce4033ef6cd2")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI1C1One2One { get; set; }

        #region Allors
        [Id("bb764aa9-4494-47f2-b91c-0d8417a8efb8")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int WorkspaceI1AllorsInteger { get; set; }

        #region Allors
        [Id("094d125f-09ec-4821-a29e-cb9560a10f7e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] WorkspaceI1C2Many2Manies { get; set; }

        #region Allors
        [Id("ebcdd8ba-c69f-48f7-b1d3-cd55e1e8d17a")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI1I1One2Manies { get; set; }

        #region Allors
        [Id("d3e29aa9-f9bb-4d2b-b234-261f179180c9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI1I1Many2Manies { get; set; }

        #region Allors
        [Id("213b1438-0ef8-4b57-b56e-96ab59f9a2e1")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool WorkspaceI1AllorsBoolean { get; set; }

        #region Allors
        [Id("c43af0de-ade2-4c8a-9720-51ad029aad2a")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal WorkspaceI1AllorsDecimal { get; set; }

        #region Allors
        [Id("06ee8b17-ff19-4d3c-9855-f505f6a483d3")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI1I12One2One { get; set; }

        #region Allors
        [Id("8ac94c9d-98b2-4574-833d-7ece4d731ec4")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI1I2One2One { get; set; }

        #region Allors
        [Id("bc4e0257-cf57-4f82-a090-20d5b4b6faa6")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI1C2One2One { get; set; }

        #region Allors
        [Id("f687488e-ae8b-4b76-ad52-550cd8698aec")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI1C1One2Manies { get; set; }

        #region Allors
        [Id("382c4068-6223-46e4-ad2a-52de9ee896ea")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] WorkspaceI1AllorsBinary { get; set; }

        #region Allors
        [Id("b25cfb4a-0feb-4078-8d38-e23b1dc8133f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI1C1Many2Manies { get; set; }

        #region Allors
        [Id("db8a5e5a-6429-49ac-af63-3bc7548ce7b4")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double WorkspaceI1AllorsDouble { get; set; }

        #region Allors
        [Id("c139a60f-d24a-4e09-82be-9e77ad09d823")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI1I1One2One { get; set; }

        #region Allors
        [Id("fbe39b32-6e34-40ed-8f09-b8e6cb2bc639")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI1C1Many2One { get; set; }

        #region Allors
        [Id("57382b43-1768-499f-bc01-a2ea0b898364")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI1I12One2Manies { get; set; }

        #region Allors
        [Id("67b8d029-7419-4f74-a142-dd1eedb86d2a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI1C2Many2One { get; set; }

        #region Allors
        [Id("b1d707c3-cb59-4fc6-9329-5c1090afb0ec")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid WorkspaceI1AllorsUnique { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("23f4ac10-8be7-4c83-8062-ee4cad174a70")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 DatabaseI1I1Many2One { get; set; }

        #region Allors
        [Id("6995bdc1-2fb4-437c-a70c-80b667f521e2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] DatabaseI1I12Many2Manies { get; set; }

        #region Allors
        [Id("09cbe477-9b12-4f14-9dbd-38e493948231")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] DatabaseI1I2Many2Manies { get; set; }

        #region Allors
        [Id("8af4dec7-1bbf-4c0f-a8a0-2b954d2d3a5f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 DatabaseI1I2Many2One { get; set; }

        #region Allors
        [Id("9f3d1b47-1390-4289-8c28-c63dbaa08594")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string DatabaseI1AllorsString { get; set; }

        #region Allors
        [Id("620dc56b-4283-4c9c-9655-a6e8d13033ea")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 DatabaseI1I12Many2One { get; set; }

        #region Allors
        [Id("c95ecb15-23f6-4a77-ba30-0bcaea9efba4")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime DatabaseI1AllorsDateTime { get; set; }

        #region Allors
        [Id("151b1aca-7782-4ed0-bb25-c67607c6cf07")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] DatabaseI1I2One2Manies { get; set; }

        #region Allors
        [Id("561fee7f-8ee5-497e-b6b2-80338ec7ca74")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] DatabaseI1C2One2Manies { get; set; }

        #region Allors
        [Id("ac516f90-2505-4347-a7b4-82ebac692bbe")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 DatabaseI1C1One2One { get; set; }

        #region Allors
        [Id("938ec6ae-6647-4027-b945-155c59141872")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int DatabaseI1AllorsInteger { get; set; }

        #region Allors
        [Id("59a20a5a-e17d-4d2c-ac4b-13aee6f6688f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] DatabaseI1C2Many2Manies { get; set; }

        #region Allors
        [Id("d89c2da4-7676-4e7c-8d74-6a0020248dfc")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] DatabaseI1I1One2Manies { get; set; }

        #region Allors
        [Id("2069bcf2-bb65-4850-bd08-525c8539edd0")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] DatabaseI1I1Many2Manies { get; set; }

        #region Allors
        [Id("cf94adcd-9657-465d-8137-37ff7fad5c9b")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool DatabaseI1AllorsBoolean { get; set; }

        #region Allors
        [Id("e1c34f10-5560-4a5e-a0d0-c3b53b5ae85a")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal DatabaseI1AllorsDecimal { get; set; }

        #region Allors
        [Id("f390eedd-9461-4d39-922e-27f39fc9c90d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 DatabaseI1I12One2One { get; set; }

        #region Allors
        [Id("964eea37-1a05-4cbf-80e5-244494056494")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 DatabaseI1I2One2One { get; set; }

        #region Allors
        [Id("8f3e3a81-982c-43de-87df-4e641bd24f02")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 DatabaseI1C2One2One { get; set; }

        #region Allors
        [Id("fe15cb96-526e-4a44-b787-7546323e798b")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] DatabaseI1C1One2Manies { get; set; }

        #region Allors
        [Id("a1ac3167-d4f7-4f6f-97f3-473d07fb1836")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] DatabaseI1AllorsBinary { get; set; }

        #region Allors
        [Id("d8731017-56e0-4540-b913-c2b1b1a9ed03")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] DatabaseI1C1Many2Manies { get; set; }

        #region Allors
        [Id("ff617b9e-de68-470c-8c5e-8cf0c3bc80ae")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double DatabaseI1AllorsDouble { get; set; }

        #region Allors
        [Id("2e54380c-fefc-4970-bca0-c26f17b0bbab")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 DatabaseI1I1One2One { get; set; }

        #region Allors
        [Id("faa39809-66c5-4070-af72-c7051ef81ce5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 DatabaseI1C1Many2One { get; set; }

        #region Allors
        [Id("42ab6e6f-498b-4fc1-b8d3-936345497e9f")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] DatabaseI1I12One2Manies { get; set; }

        #region Allors
        [Id("6448e268-9e4c-4b0d-ac81-f743825a00ee")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 DatabaseI1C2Many2One { get; set; }

        #region Allors
        [Id("67d62690-c01a-47c2-a225-182dd66c4bf9")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid DatabaseI1AllorsUnique { get; set; }

        #endregion

        #region Allors
        [Id("f2064857-1313-4053-8bf7-36584702c9fc")]
        #endregion
        void InterfaceMethod();
    }
}
