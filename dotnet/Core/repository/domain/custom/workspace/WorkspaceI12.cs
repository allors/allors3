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
    [Id("843ebd83-6fa3-42bf-ad7e-32500039e470")]
    #endregion
    public partial interface WorkspaceI12 : S12
    {
        #region Workspace

        #region Allors
        [Id("a1265d19-9f34-457c-866b-18b4f687b1d5")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] WorkspaceI12AllorsBinary { get; set; }

        #region Allors
        [Id("a0979ae4-dd5d-4eeb-a2e8-c2377c316cd3")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI12C2One2One { get; set; }

        #region Allors
        [Id("33a54ef9-6529-43e0-b032-0e8b520a55d5")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double WorkspaceI12AllorsDouble { get; set; }

        #region Allors
        [Id("b49544d5-b95e-44e6-8bb6-e3c036f647e1")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI12I1Many2One { get; set; }

        #region Allors
        [Id("d21a2bbe-b4c3-4115-9dd7-abb175c31cc1")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string WorkspaceI12AllorsString { get; set; }

        #region Allors
        [Id("e7b71ea6-b5b8-4ae8-aca9-de4223390340")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI12I12Many2Manies { get; set; }

        #region Allors
        [Id("dc64824e-610c-472d-9175-66fc997c5e82")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal WorkspaceI12AllorsDecimal { get; set; }

        #region Allors
        [Id("fdeb9240-1476-4cdc-b7bc-f234a8463ac4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI12I2Many2Manies { get; set; }

        #region Allors
        [Id("8bae138d-2b60-4cb4-83f0-8c45cf06ecc8")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] WorkspaceI12C2Many2Manies { get; set; }

        #region Allors
        [Id("c27bba70-a390-4cef-a3be-034751e7ed9c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI12I1Many2Manies { get; set; }

        #region Allors
        [Id("a35c5681-6c3f-4918-b5c6-2d4d8a216111")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceI12I12One2Manies { get; set; }

        #region Allors
        [Id("3b6932cc-96d3-4483-ac89-ac7c8b636600")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string WorkspaceName { get; set; }

        #region Allors
        [Id("aa854ff8-d50a-42ba-880e-72425238a498")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI12C1Many2Manies { get; set; }

        #region Allors
        [Id("bd365f76-a43d-4f18-887e-efbed54c4c62")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI12I2Many2One { get; set; }

        #region Allors
        [Id("fad23bbd-a7d1-4c74-811f-cc8616106420")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid WorkspaceI12AllorsUnique { get; set; }

        #region Allors
        [Id("a2c56bed-e805-4e26-9da0-eef439a3865c")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int WorkspaceI12AllorsInteger { get; set; }

        #region Allors
        [Id("1ee55c7e-3228-4d99-a739-3cb7e69bfe25")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] WorkspaceI12I1One2Manies { get; set; }

        #region Allors
        [Id("679fa42e-dd87-4fd3-b2a8-c6310ae81243")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI12C1One2One { get; set; }

        #region Allors
        [Id("a11496e6-9be7-4846-82b9-dde078adf6f8")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI12I12One2One { get; set; }

        #region Allors
        [Id("ec66b4a3-a533-4622-b5ab-4f7627b01f86")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 WorkspaceI12I2One2One { get; set; }

        #region Allors
        [Id("dd1a5b5c-5018-40f0-ba8d-82a674f70d24")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] WorkspaceDependencies { get; set; }

        #region Allors
        [Id("22058203-b18d-4fa8-87ce-308ee7bc4221")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] WorkspaceI12I2One2Manies { get; set; }

        #region Allors
        [Id("f8199a08-860f-4289-942d-d895b32935aa")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 WorkspaceI12C2Many2One { get; set; }

        #region Allors
        [Id("4f16ab82-24b7-44fa-b2fe-37ef6b6699fe")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 WorkspaceI12I12Many2One { get; set; }

        #region Allors
        [Id("96daeac6-846f-436c-aa89-8c9d3673f48b")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool WorkspaceI12AllorsBoolean { get; set; }

        #region Allors
        [Id("fc7439da-a840-4afc-b82a-f17144bedcd4")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 WorkspaceI12I1One2One { get; set; }

        #region Allors
        [Id("9af03720-6dc2-4350-b40e-f3ea14fc07c1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] WorkspaceI12C1One2Manies { get; set; }

        #region Allors
        [Id("4b79aa06-354d-4548-bfcf-7792a5421c46")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 WorkspaceI12C1Many2One { get; set; }

        #region Allors
        [Id("4b73b1aa-9de9-499b-b374-cb85b9a72f88")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime WorkspaceI12AllorsDateTime { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("a5aafcdc-7b00-4731-8366-8626bc8397cc")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        byte[] DatabaseI12AllorsBinary { get; set; }

        #region Allors
        [Id("d23e252d-881f-403d-a9d7-1d92ea6ca9f3")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 DatabaseI12C2One2One { get; set; }

        #region Allors
        [Id("e4520fdf-d3aa-407a-a48f-7b81ad623e98")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        double DatabaseI12AllorsDouble { get; set; }

        #region Allors
        [Id("32d24da3-dec7-464c-9e3b-0d4ff3d910ec")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 DatabaseI12I1Many2One { get; set; }

        #region Allors
        [Id("9df2b88d-f0f0-49a8-81cc-4ee774c621c8")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string DatabaseI12AllorsString { get; set; }

        #region Allors
        [Id("75fd91a8-d279-4e94-ad92-df129f42fb7b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] DatabaseI12I12Many2Manies { get; set; }

        #region Allors
        [Id("be1015b3-a09b-4620-8a04-058037353972")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        decimal DatabaseI12AllorsDecimal { get; set; }

        #region Allors
        [Id("80e13aaa-4caa-4c7f-a073-2bc18f9aafe2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] DatabaseI12I2Many2Manies { get; set; }

        #region Allors
        [Id("80af9efd-91b4-4a04-8476-3aeb953e7415")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2[] DatabaseI12C2Many2Manies { get; set; }

        #region Allors
        [Id("e6f01f6b-fdc1-4507-a2a4-badeb8ba5c10")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] DatabaseI12I1Many2Manies { get; set; }

        #region Allors
        [Id("6ec1e7fe-f2d4-49a6-bef4-5dadab6f65a5")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] DatabaseI12I12One2Manies { get; set; }

        #region Allors
        [Id("cca17601-30dc-4b9c-a0e0-b77b53c6bee3")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        string DatabaseName { get; set; }

        #region Allors
        [Id("36405afb-2a62-4f12-9009-9a17eb7bdaab")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] DatabaseI12C1Many2Manies { get; set; }

        #region Allors
        [Id("8b9c5cfe-bc70-4bc3-ae32-acb0d2eacfb4")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 DatabaseI12I2Many2One { get; set; }

        #region Allors
        [Id("ec43d122-83d9-46ad-82c2-868ef4839f42")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        Guid DatabaseI12AllorsUnique { get; set; }

        #region Allors
        [Id("75198849-a9d1-49bf-96b3-b44baf5ad3ed")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        int DatabaseI12AllorsInteger { get; set; }

        #region Allors
        [Id("60560136-00d2-4b70-8536-0d968fedd5cb")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1[] DatabaseI12I1One2Manies { get; set; }

        #region Allors
        [Id("f1428b1f-4402-434b-b175-558da6f664da")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 DatabaseI12C1One2One { get; set; }

        #region Allors
        [Id("939f5687-79c3-4527-88f2-192d80f6f139")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 DatabaseI12I12One2One { get; set; }

        #region Allors
        [Id("b2a8ddce-7e17-43c8-9d19-1184915a20eb")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2 DatabaseI12I2One2One { get; set; }

        #region Allors
        [Id("1a8dbbc0-e5a3-47f6-a0b4-d27e931c3978")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12[] DatabaseDependencies { get; set; }

        #region Allors
        [Id("ebc81eba-5e6e-4d9f-938d-b133e28f0415")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I2[] DatabaseI12I2One2Manies { get; set; }

        #region Allors
        [Id("a515c086-b024-4007-92fe-c042255a3b39")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C2 DatabaseI12C2Many2One { get; set; }

        #region Allors
        [Id("30999318-1298-48ab-8cf0-8f375c82ef30")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I12 DatabaseI12I12Many2One { get; set; }

        #region Allors
        [Id("9b1e39e4-0a01-4739-83ec-a702367c0b29")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        bool DatabaseI12AllorsBoolean { get; set; }

        #region Allors
        [Id("658beec7-15ba-421f-9fe5-c33ed866bc4f")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        I1 DatabaseI12I1One2One { get; set; }

        #region Allors
        [Id("0e34eaf9-a271-4d42-a226-a658a0f5c176")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1[] DatabaseI12C1One2Manies { get; set; }

        #region Allors
        [Id("57e2f661-4672-412e-a876-9af9a9376ec5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        C1 DatabaseI12C1Many2One { get; set; }

        #region Allors
        [Id("dc887e60-a39a-40e3-a4d2-a6db7ecdcb27")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        DateTime DatabaseI12AllorsDateTime { get; set; }

        #endregion
    }
}
