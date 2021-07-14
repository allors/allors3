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
        #region Session

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
        SessionC1[] SessionI12SessionC1Many2Manies { get; set; }

        #region Allors
        [Id("666cb267-c669-4300-9d1d-a5c93780088e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1[] SessionI12SessionC1One2Manies { get; set; }

        #region Allors
        [Id("5b7da3fc-41ca-46be-9442-7ff3e8289301")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI12SessionC1Many2One { get; set; }

        #region Allors
        [Id("1ba88357-1460-4bcf-ae80-7618781b99f5")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI12SessionC1One2One { get; set; }

        #region Allors
        [Id("6eb50c27-5fb9-493b-a70d-f916b97c3844")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI12SessionC2One2One { get; set; }

        #region Allors
        [Id("f791ef10-1733-4774-a172-bf1d59c61ff4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2[] SessionI12SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("c9535c5d-f171-49e9-a948-f1e825053f54")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI12SessionC2Many2One { get; set; }

        #region Allors
        [Id("b473d477-ac03-4586-a512-d53ce90e1a0f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1 SessionI12SessionI1Many2One { get; set; }

        #region Allors
        [Id("a95dfa52-c118-42b6-b129-24b58ea69439")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1 SessionI12SessionI1One2One { get; set; }

        #region Allors
        [Id("81f8568b-2e07-44cf-b452-69447b0b0db1")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI12SessionI1One2Manies { get; set; }

        #region Allors
        [Id("fa270183-f6d0-44f6-b92a-1d8181459c2d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI12SessionI1Many2Manies { get; set; }

        #region Allors
        [Id("19cbfeff-bf16-4380-915e-930fcb0713d9")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2[] SessionI12SessionI2Many2Manies { get; set; }

        #region Allors
        [Id("9b7ebccb-2184-4844-aeb6-be4cdba36b5f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI12SessionI2Many2One { get; set; }

        #region Allors
        [Id("0bbc5c82-0d61-493a-a6bf-fdc04d077a65")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI12SessionI2One2One { get; set; }

        #region Allors
        [Id("99f0230b-14ed-429d-a3c8-ef7fafbd69d9")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2[] SessionI12SessionI2One2Manies { get; set; }

        #region Allors
        [Id("dfc02521-be67-42e5-aff2-3fe32e395621")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionI12SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("7a33c7af-e88f-4f92-8248-78cbe379ec40")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionI12SessionI12One2Manies { get; set; }

        #region Allors
        [Id("254a47ea-a20b-4b93-a7af-0f45b92ed240")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12 SessionI12SessionI12One2One { get; set; }

        #region Allors
        [Id("661c9e17-b056-40bd-bdd1-d134f1ebef6f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionDependencies { get; set; }

        #region Allors
        [Id("c5436dc2-f903-4734-b2d5-74e0545e0d19")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12 SessionI12SessionI12Many2One { get; set; }

        #endregion

        #region Workspace

        #region Allors
        [Id("ac21702b-2bc0-48d0-a2ee-511894d814e2")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] WorkspaceI12AllorsBinary { get; set; }

        #region Allors
        [Id("bfb82d60-f155-47a7-9fca-140ceabbc471")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int WorkspaceI12AllorsInteger { get; set; }

        #region Allors
        [Id("9843dc79-e6a7-4611-80e7-81ac86b83ba2")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string WorkspaceI12AllorsString { get; set; }

        #region Allors
        [Id("04fac4b1-576f-40aa-8b67-e2d8552c20fd")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double WorkspaceI12AllorsDouble { get; set; }

        #region Allors
        [Id("3a9d9a94-e101-4c6b-bebd-529c2247dd3c")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal WorkspaceI12AllorsDecimal { get; set; }

        #region Allors
        [Id("d13d3b23-bd18-4175-bb7d-5076ef9dacf8")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid WorkspaceI12AllorsUnique { get; set; }

        #region Allors
        [Id("5f1b0ef9-9143-4e04-8493-db413567bd0a")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime WorkspaceI12AllorsDateTime { get; set; }

        #region Allors
        [Id("763e7da5-6bf7-4831-8159-9a5c3e8080f2")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool WorkspaceI12AllorsBoolean { get; set; }

        #region Allors
        [Id("8e4dec47-52ad-4249-934a-4aff3314de9c")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string WorkspaceName { get; set; }

        #region Allors
        [Id("7f2c6fe9-7974-4fc5-b51d-6e876fb7ae12")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI12WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("539bb865-b4f5-41af-8cd8-19601dd4b567")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI12WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("c1418ba6-3447-43aa-8436-3335506ea6a4")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI12WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("c5f7f61b-845a-4e27-a7ca-bf9e56d9d299")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI12WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("e1513b56-8b4b-4e04-920b-cbd09ec47d30")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI12WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("dab340f2-ba80-4407-b9b2-fd968578eba3")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2[] SessionI12WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("0e92c739-4c3b-457a-a6b2-b50da566dab8")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI12WorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("6fd648bf-756b-4f3e-96e3-ab8353e64a30")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI12WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("4681bc0d-0e0f-42ab-a113-7596881a7cbc")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI12WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("bcf3214a-a3db-4838-aa4b-3bf2fa1c5845")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI12WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("8bb0137e-66e1-4ac6-a97d-ad11dc9b9057")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI12WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("00ad31bd-a803-48d1-8077-dc213d718627")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI12WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("1446f3ca-2330-4500-bd6b-972c2d9f6022")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI12WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("029e9c12-03b8-4caf-abb9-7c7cc6cf13a2")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI12WorkspaceI2One2One { get; set; }

        #region Allors
        [Id("966d31e5-15af-4a9c-9974-899e4002ea28")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI12WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("b93d4692-bf96-4b63-9950-f629fe78979e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI12WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("46ab9551-3276-40c0-bfd2-34407a55754d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI12WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("7ca7847c-66d4-44b4-8268-fc3e97cad498")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI12WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("7db01151-1732-4cfa-aac4-ff1c2109eee2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] WorkspaceDependencies { get; set; }

        #region Allors
        [Id("f3141e88-50d6-4a5e-8a84-c3637cbd5812")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI12WorkspaceI12Many2One { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("8c646b43-526f-40c9-8b6e-459232ac545f")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] DatabaseI12AllorsBinary { get; set; }

        #region Allors
        [Id("d28d2633-d4d1-4d31-9c55-8148103c4213")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int DatabaseI12AllorsInteger { get; set; }

        #region Allors
        [Id("1bd93c5b-68d3-43fd-8915-d2f1791b7e92")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string DatabaseI12AllorsString { get; set; }

        #region Allors
        [Id("563d2497-2ce1-4c3e-8627-24cfe3ea42d1")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double DatabaseI12AllorsDouble { get; set; }

        #region Allors
        [Id("d204bf4d-a282-4c86-ac72-416ebc01c093")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal DatabaseI12AllorsDecimal { get; set; }

        #region Allors
        [Id("df33caeb-0cc4-4ddb-89cf-d31b1ad2fb82")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid DatabaseI12AllorsUnique { get; set; }

        #region Allors
        [Id("ead4b76d-b816-441f-bbb8-649d9f33c839")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime DatabaseI12AllorsDateTime { get; set; }

        #region Allors
        [Id("90f6f1ad-ac37-4e85-8fea-c3e6ee24c327")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool DatabaseI12AllorsBoolean { get; set; }

        #region Allors
        [Id("647464a9-472a-4245-bcd9-e9adf49cabe2")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string DatabaseName { get; set; }

        #region Allors
        [Id("a0b69edb-0be2-4e2f-96b4-16d79a00e8e1")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI12DatabaseC1Many2Manies { get; set; }

        #region Allors
        [Id("27de768c-ee0b-4627-888a-4f7833d6f35b")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI12DatabaseC1One2Manies { get; set; }

        #region Allors
        [Id("eb9f8fe8-2f46-41d7-90ad-e949901271bb")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI12DatabaseC1Many2One { get; set; }

        #region Allors
        [Id("f6004804-b7fb-4ab9-9ff3-fa2684473580")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI12DatabaseC1One2One { get; set; }

        #region Allors
        [Id("00495922-1442-4abd-9a26-74d139b10780")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI12DatabaseC2One2One { get; set; }

        #region Allors
        [Id("07d3c3b5-88fd-49ee-b379-f2cd9b75d0af")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI12DatabaseC2Many2Manies { get; set; }

        #region Allors
        [Id("2f1635e9-7f7f-4b3c-a54a-31c91b4baa6e")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI12DatabaseC2Many2One { get; set; }

        #region Allors
        [Id("d0f14037-b600-49c1-968c-3ae4a290dc1f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI12DatabaseI1Many2One { get; set; }

        #region Allors
        [Id("6a795842-1b49-4ebd-b542-d22cfb46f06c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI12DatabaseI1One2One { get; set; }

        #region Allors
        [Id("14258e1a-e139-40fc-995c-c3d05115ec00")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI12DatabaseI1One2Manies { get; set; }

        #region Allors
        [Id("f98f1e67-dfbd-4997-9d61-47605ee8e1db")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI12DatabaseI1Many2Manies { get; set; }

        #region Allors
        [Id("2bb04ca8-37cc-463b-b17a-477deb1a224e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI12DatabaseI2Many2Manies { get; set; }

        #region Allors
        [Id("7e2a9cc6-9a57-46f1-93fc-c91bf1ef951c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI12DatabaseI2Many2One { get; set; }

        #region Allors
        [Id("c3503ccf-1622-4ff3-8eb3-56f6ecccc5ae")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI12DatabaseI2One2One { get; set; }

        #region Allors
        [Id("a4446250-14d2-451e-8610-67925ca8124d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI12DatabaseI2One2Manies { get; set; }

        #region Allors
        [Id("39987b99-7cbc-49a1-84b1-e176ef05fa8c")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI12DatabaseI12Many2Manies { get; set; }

        #region Allors
        [Id("34881815-b0c9-4e80-a0e6-0981e961fb3a")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI12DatabaseI12One2Manies { get; set; }

        #region Allors
        [Id("d98d3744-db24-4bab-a0fc-64f55751cf6e")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI12DatabaseI12One2One { get; set; }

        #region Allors
        [Id("55d9fd31-eb81-48cf-a363-e0534fc298da")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] DatabaseDependencies { get; set; }

        #region Allors
        [Id("efd214c3-e536-4a02-af5e-284b2abecac9")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI12DatabaseI12Many2One { get; set; }

        #endregion
    }
}
