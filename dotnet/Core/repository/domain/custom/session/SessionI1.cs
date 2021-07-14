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
        #region Session

        #region Allors
        [Id("82113f39-2273-45d8-91be-237f3080724b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1 SessionI1SessionI1Many2One { get; set; }

        #region Allors
        [Id("3ee47288-2f5d-4989-ad9e-f04987dffef5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionI1SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("cebd4b4a-aa91-42c6-b002-423a22a725dd")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2[] SessionI1SessionI2Many2Manies { get; set; }

        #region Allors
        [Id("68b31d55-9045-4d69-9acc-049d37c131dd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI1SessionI2Many2One { get; set; }

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
        SessionI12 SessionI1SessionI12Many2One { get; set; }

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
        SessionI2[] SessionI1SessionI2One2Manies { get; set; }

        #region Allors
        [Id("cab68c9b-de6b-476f-90a0-dba731c85090")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2[] SessionI1SessionC2One2Manies { get; set; }

        #region Allors
        [Id("5ea7b03c-e156-4730-ba70-2cf2ac3d8c2c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI1SessionC1One2One { get; set; }

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
        SessionC2[] SessionI1SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("ed5dea24-94bd-43f7-b8a3-0915d68d3ca2")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI1SessionI1One2Manies { get; set; }

        #region Allors
        [Id("4f64bf79-5a6f-4837-bd23-e0c06406efa9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI1SessionI1Many2Manies { get; set; }

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
        SessionI12 SessionI1SessionI12One2One { get; set; }

        #region Allors
        [Id("f7e48d3b-4830-4f15-af88-e82022319b3d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI1SessionI2One2One { get; set; }

        #region Allors
        [Id("aa7c893f-3b39-4633-be67-7e023cb38f07")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI1SessionC2One2One { get; set; }

        #region Allors
        [Id("71bda511-e99f-496a-8337-3ddc6f57e8d1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1[] SessionI1SessionC1One2Manies { get; set; }

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
        SessionC1[] SessionI1SessionC1Many2Manies { get; set; }

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
        SessionI1 SessionI1SessionI1One2One { get; set; }

        #region Allors
        [Id("ded1febe-b3ea-41d8-956f-6bea87c79dfd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI1SessionC1Many2One { get; set; }

        #region Allors
        [Id("e4be4987-77d8-4f7e-a5f2-b476489729b9")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionI1SessionI12One2Manies { get; set; }

        #region Allors
        [Id("bc66dc84-920c-4e8f-86f8-a406773d4b3f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI1SessionC2Many2One { get; set; }

        #region Allors
        [Id("9ac858f8-6f62-4e3c-860a-1494c019cc03")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid SessionI1AllorsUnique { get; set; }

        #endregion

        #region Workspace

        #region Allors
        [Id("5c3375fb-d9cf-4a18-bb35-35aad2de7bfe")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI1WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("6db08e77-9802-403a-9760-a2a316d72f42")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI1WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("e65ec301-a8b2-41c5-995a-e8e7913a716b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI1WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("146e280c-5615-4f99-a1d4-4d81fc1e29b1")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI1WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("bcf68917-2b46-44bd-b259-e6d1eef60151")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string WorkspaceI1AllorsString { get; set; }

        #region Allors
        [Id("58dc782b-a890-4747-bb8f-7cb8aeb048a0")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI1WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("543dad4b-0310-4366-940c-ed968a45da5f")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime WorkspaceI1AllorsDateTime { get; set; }

        #region Allors
        [Id("dc585670-3726-4865-9425-d73be7cee68e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI1WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("4b3ce7d7-f18f-46ee-9f06-adc60a500480")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2[] SessionI1WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("927fab54-56d5-40d9-91f9-2c07437e4482")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI1WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("72f0be5b-aee0-4e23-8162-40814bbc916d")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int WorkspaceI1AllorsInteger { get; set; }

        #region Allors
        [Id("7612eda3-beb2-4a64-b738-c32f658f2e0d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2[] SessionI1WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("8766911e-fbcd-47ac-bc5c-50f665dd99da")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI1WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("82078307-e0b7-4690-89b1-2fa51def556f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI1WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("3e8c1ce6-b6e0-4637-a03a-22448952c33d")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool WorkspaceI1AllorsBoolean { get; set; }

        #region Allors
        [Id("c4fcb4fc-dcad-4658-95ce-4a8ac245c562")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal WorkspaceI1AllorsDecimal { get; set; }

        #region Allors
        [Id("af25ec52-fc49-4e67-bc2a-c5c4828ce956")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI1WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("b046602f-bb4b-4308-bbca-48cd7509b80d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI1WorkspaceI2One2One { get; set; }

        #region Allors
        [Id("896d8dcf-2af9-4467-8482-af3f86885b46")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI1WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("f4967e64-222c-4bef-95cd-aded1f488b76")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI1WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("b46a29a0-7905-4446-86b8-1c86c91b0dbb")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] WorkspaceI1AllorsBinary { get; set; }

        #region Allors
        [Id("31289494-df24-4bc7-b4c3-b656cb366101")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI1WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("bfca880f-8438-40ef-98c0-d8d99e8f2b6f")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double WorkspaceI1AllorsDouble { get; set; }

        #region Allors
        [Id("b82e1156-7cac-499f-a537-da90906f60b0")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI1WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("af814b80-3236-497c-8821-58a2f90ffed2")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI1WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("1460d65b-88cd-4544-be60-147b289a89f8")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI1WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("49b96468-92bf-4b5e-81a7-4fee505f4efc")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI1WorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("4bbceece-92f6-4904-983b-ae4922b2edfd")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid WorkspaceI1AllorsUnique { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("67559b35-30f8-4126-a136-33d096afcf40")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI1DatabaseI1Many2One { get; set; }

        #region Allors
        [Id("3a32fe64-5cb0-4606-b4df-1cfc2318858d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI1DatabaseI12Many2Manies { get; set; }

        #region Allors
        [Id("adee9642-c7a5-4d89-82e4-3150d6f82c62")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI1DatabaseI2Many2Manies { get; set; }

        #region Allors
        [Id("b5cc69ed-1326-47a5-9c23-5d66927f0f89")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI1DatabaseI2Many2One { get; set; }

        #region Allors
        [Id("719dfe3e-d64d-4e3f-88a2-5522ddcad05f")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string DatabaseI1AllorsString { get; set; }

        #region Allors
        [Id("b4a34c27-9c81-45ae-adcd-9705d59ff2b6")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI1DatabaseI12Many2One { get; set; }

        #region Allors
        [Id("7a1c6717-c498-438b-8880-c3e855b3c7e2")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime DatabaseI1AllorsDateTime { get; set; }

        #region Allors
        [Id("9e598e21-32a1-4411-add6-81de4c57abc1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI1DatabaseI2One2Manies { get; set; }

        #region Allors
        [Id("f27ea7ba-c38f-4945-a0f4-2405f4755a0d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI1DatabaseC2One2Manies { get; set; }

        #region Allors
        [Id("bdf01fac-2df4-4152-aea7-7dbd61a23326")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI1DatabaseC1One2One { get; set; }

        #region Allors
        [Id("8367a1e7-bf3f-44f3-9cf7-1f12b878dc1b")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int DatabaseI1AllorsInteger { get; set; }

        #region Allors
        [Id("088cddd1-cc5c-4cc7-99e4-a8ca835501ac")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI1DatabaseC2Many2Manies { get; set; }

        #region Allors
        [Id("66a67661-01b8-41ce-b9a1-6960826dbb79")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI1DatabaseI1One2Manies { get; set; }

        #region Allors
        [Id("951409a7-1397-46a9-aefe-5f5845d5abfe")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI1DatabaseI1Many2Manies { get; set; }

        #region Allors
        [Id("2dfd9ef5-3e5e-45db-b039-d04dca0ef581")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool DatabaseI1AllorsBoolean { get; set; }

        #region Allors
        [Id("679aa8c6-f063-430a-8a51-a036d9dcd866")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal DatabaseI1AllorsDecimal { get; set; }

        #region Allors
        [Id("1fa91e7a-a51b-4987-85df-82175cce3e8e")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI1DatabaseI12One2One { get; set; }

        #region Allors
        [Id("86230245-31a7-47cf-864c-c9b98addf04d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI1DatabaseI2One2One { get; set; }

        #region Allors
        [Id("61571fff-adb7-486c-b337-610c48afb100")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI1DatabaseC2One2One { get; set; }

        #region Allors
        [Id("b0f35d7d-1bb3-4698-a7b7-ee3e1c8a93a7")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI1DatabaseC1One2Manies { get; set; }

        #region Allors
        [Id("8dd014ef-fe06-4c56-b718-3cdacb96f4c7")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] DatabaseI1AllorsBinary { get; set; }

        #region Allors
        [Id("4a56cc19-e70b-4f23-8ab0-0662f20d577c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI1DatabaseC1Many2Manies { get; set; }

        #region Allors
        [Id("169e5290-99e1-4480-8ffe-056b4152faad")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double DatabaseI1AllorsDouble { get; set; }

        #region Allors
        [Id("a909ee02-15a7-4f76-9d16-94398c512cf9")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI1DatabaseI1One2One { get; set; }

        #region Allors
        [Id("c4ce9197-3b6c-46f5-be4e-bc719c450ee4")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI1DatabaseC1Many2One { get; set; }

        #region Allors
        [Id("af794e61-818d-45ad-a25f-d6dcd940e4a3")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI1DatabaseI12One2Manies { get; set; }

        #region Allors
        [Id("00e16fe0-14c9-4da0-a1d7-89b1601684d5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI1DatabaseC2Many2One { get; set; }

        #region Allors
        [Id("c0eb7b5a-83d7-43c1-a59d-555a7763c36d")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid DatabaseI1AllorsUnique { get; set; }

        #endregion

        #region Allors
        [Id("e14c40cb-1278-47c6-928b-aa963ecf23ac")]
        #endregion
        void InterfaceMethod();
    }
}
