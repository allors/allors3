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
        #region Session

        #region Allors
        [Id("24273b38-3fc8-473b-8acd-4969ed58a3dd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI2SessionI2Many2One { get; set; }

        #region Allors
        [Id("61d29099-313b-40b1-ba88-62caa224b82c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI2SessionC1Many2One { get; set; }

        #region Allors
        [Id("ea880241-22b2-4bc4-a5f4-371f0fa9b96f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12 SessionI2SessionI12Many2One { get; set; }

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
        SessionC1[] SessionI2SessionC1One2Manies { get; set; }

        #region Allors
        [Id("4ad55fe8-bd51-4b23-a942-a0644cd2d3ee")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1 SessionI2SessionC1One2One { get; set; }

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
        SessionI2[] SessionI2SessionI2Many2Manies { get; set; }

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
        SessionI1 SessionI2SessionI1Many2One { get; set; }

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
        SessionI12[] SessionI2SessionI12One2Manies { get; set; }

        #region Allors
        [Id("e470eea2-4524-41fe-9efa-1ee5a7173faa")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12 SessionI2SessionI12One2One { get; set; }

        #region Allors
        [Id("461db928-4347-4748-9f1f-5fd75236ccd7")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2[] SessionI2SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("8f0da928-54e4-48b0-a6a8-17f1e9b21dfb")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI2SessionI1Many2Manies { get; set; }

        #region Allors
        [Id("6e2cb6db-3047-4cb4-9478-1676ecc7e4a3")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI2SessionC2Many2One { get; set; }

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
        SessionC2[] SessionI2SessionC2One2Manies { get; set; }

        #region Allors
        [Id("6bf37103-06e8-4648-be66-d675c5c892f1")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1 SessionI2SessionI1One2One { get; set; }

        #region Allors
        [Id("72585271-8774-4f8a-9069-d7a632c395ea")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI1[] SessionI2SessionI1One2Manies { get; set; }

        #region Allors
        [Id("afe7d1a6-ac93-472d-abd7-53d7d892b8cf")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI12[] SessionI2SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("95e0e2c0-c37f-4e65-a06c-9b4935edf946")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionI2 SessionI2SessionI2One2One { get; set; }

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
        SessionI2[] SessionI2SessionI2One2Manies { get; set; }

        #region Allors
        [Id("ab624c65-aed4-43dc-b4c1-5b2bfaede713")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC1[] SessionI2SessionC1Many2Manies { get; set; }

        #region Allors
        [Id("e14960fd-b6aa-4b57-8bef-fce918665966")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        SessionC2 SessionI2SessionC2One2One { get; set; }

        #region Allors
        [Id("c17b39d9-d20e-4b9f-856c-e0b6e2aef2e6")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double SessionI2AllorsDouble { get; set; }

        #endregion

        #region Workspace

        #region Allors
        [Id("8eab0d04-ff7a-4855-b5b3-dcdfcc6556b3")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI2WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("ee35b87b-aedd-4ed3-8d29-10341b166acf")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI2WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("cb8cc092-f21d-49d5-b70d-59bb6cf56309")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI2WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("a26a7a06-2448-4ec3-88f9-8eb62814a580")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool WorkspaceI2AllorsBoolean { get; set; }

        #region Allors
        [Id("b449a745-bff1-4f2f-bf70-da59c975b81c")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI2WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("437a271a-c2a7-4e1a-befc-f26e7fc41f2b")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1 SessionI2WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("81c06516-03f3-4e9c-92e9-2c1236110f76")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal WorkspaceI2AllorsDecimal { get; set; }

        #region Allors
        [Id("678290e3-8655-4b67-9e5b-45efe1fcb556")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI2WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("fdc1749c-3f63-46c8-8c3b-1dc6c3ed2485")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] WorkspaceI2AllorsBinary { get; set; }

        #region Allors
        [Id("2ffc8532-211c-4e70-ae1a-3e4b46126b96")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid WorkspaceI2AllorsUnique { get; set; }

        #region Allors
        [Id("9be2ee0c-e2b7-4a2c-bf09-bc88a560f10c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI2WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("6101c7b8-2ca4-4070-9d82-d96fb41c0666")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime WorkspaceI2AllorsDateTime { get; set; }

        #region Allors
        [Id("9c771cf0-c666-4f14-bb43-1df85f273035")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI2WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("83380b35-f309-468c-8810-282251f40b20")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12 SessionI2WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("49449bbb-124b-44ca-bfe1-9fd451d2cde1")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2[] SessionI2WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("16008e73-5b77-45ec-be8b-d5384120125f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI2WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("29ef0e6f-0c4c-4745-91d0-b2c8d9f6ea45")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI2WorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("0d3dcd53-1e79-4ed1-a49d-38e096633799")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string WorkspaceI2AllorsString { get; set; }

        #region Allors
        [Id("6ff9d2eb-e65b-4d0f-8b12-655d6ab2bbe4")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2[] SessionI2WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("9c5f22de-22c9-44b1-a5e3-1c6e4320db9f")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1 SessionI2WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("d2276c31-9ba8-48fb-852b-6552e7809b9d")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI1[] SessionI2WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("eda28951-0c2d-4ec0-af99-81427eeb721a")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI12[] SessionI2WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("a2a9f38c-678f-4cc0-955a-5927589fae43")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2 SessionI2WorkspaceI2One2One { get; set; }

        #region Allors
        [Id("a1c6c64b-653b-43d5-88d0-3e2390f48676")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int WorkspaceI2AllorsInteger { get; set; }

        #region Allors
        [Id("8eb454bd-2645-4fbd-a1a4-c61ecb9244cf")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceI2[] SessionI2WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("e32bab6d-423e-47a9-9c0e-ced1d856b4a4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC1[] SessionI2WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("cf3a852d-cdc1-4423-8ca3-69bcd2b81016")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        WorkspaceC2 SessionI2WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("4ecfa852-991a-41a4-99db-b972e7e31e02")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double WorkspaceI2AllorsDouble { get; set; }

        #endregion

        #region Database

        #region Allors
        [Id("a71e81bc-0f71-4461-b6f2-89460937f59f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI2DatabaseI2Many2One { get; set; }

        #region Allors
        [Id("f5a94dff-2c4c-4f30-94a8-ba700933fbb0")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI2DatabaseC1Many2One { get; set; }

        #region Allors
        [Id("f6e239f2-f4c7-4dcd-b881-2b78cee803c9")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI2DatabaseI12Many2One { get; set; }

        #region Allors
        [Id("705d0f2e-5f9f-4498-8225-d255b248509b")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        bool DatabaseI2AllorsBoolean { get; set; }

        #region Allors
        [Id("f87b0cb7-ed0d-4737-a74f-660c19c350d0")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI2DatabaseC1One2Manies { get; set; }

        #region Allors
        [Id("347288df-df37-4417-ab5a-2c89c432cb0d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1 SessionI2DatabaseC1One2One { get; set; }

        #region Allors
        [Id("b1215873-a9df-4f04-976d-8c6320237098")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        decimal DatabaseI2AllorsDecimal { get; set; }

        #region Allors
        [Id("3dec40e4-f83b-44d2-81d7-d124c07591cf")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI2DatabaseI2Many2Manies { get; set; }

        #region Allors
        [Id("3190c2bc-c191-47e5-afd3-c7cc641bd95e")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        byte[] DatabaseI2AllorsBinary { get; set; }

        #region Allors
        [Id("bfc3e05e-1786-40f9-9f14-5e86a7c3d706")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        Guid DatabaseI2AllorsUnique { get; set; }

        #region Allors
        [Id("aa1a69a1-4d76-46a7-91fe-ad074b9f0dc2")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI2DatabaseI1Many2One { get; set; }

        #region Allors
        [Id("55baf4a5-40d6-4474-a898-f1d08eb1935d")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        DateTime DatabaseI2AllorsDateTime { get; set; }

        #region Allors
        [Id("31ac0628-84af-4a8d-8b8a-8d9bfd90e26e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI2DatabaseI12One2Manies { get; set; }

        #region Allors
        [Id("988eee9e-a47d-4c96-8409-fdd3524dc11e")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12 SessionI2DatabaseI12One2One { get; set; }

        #region Allors
        [Id("7d520cd9-4ef0-4583-a8da-4ad5a6dddf0e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI2DatabaseC2Many2Manies { get; set; }

        #region Allors
        [Id("6b14279b-5741-4d64-931c-ffb225bb83e8")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI2DatabaseI1Many2Manies { get; set; }

        #region Allors
        [Id("12c56c28-4592-4894-a907-3e6ec51acc49")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI2DatabaseC2Many2One { get; set; }

        #region Allors
        [Id("b930ee57-b3f1-4d13-95e1-f558e3a001d8")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        string DatabaseI2AllorsString { get; set; }

        #region Allors
        [Id("b1388f15-92d4-40b8-83a8-3bc24f36a230")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2[] SessionI2DatabaseC2One2Manies { get; set; }

        #region Allors
        [Id("1468404b-5111-4969-be30-dca7e40bdac7")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1 SessionI2DatabaseI1One2One { get; set; }

        #region Allors
        [Id("9940f44e-e56a-45cd-a7e0-d27ad9d13f16")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I1[] SessionI2DatabaseI1One2Manies { get; set; }

        #region Allors
        [Id("965955c6-8d10-441a-be72-64fcecb132c5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I12[] SessionI2DatabaseI12Many2Manies { get; set; }

        #region Allors
        [Id("1b0fd156-2981-4d0c-a745-f182170ae008")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2 SessionI2DatabaseI2One2One { get; set; }

        #region Allors
        [Id("fe909b81-a50b-4f21-ab7c-61838e272a37")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        int DatabaseI2AllorsInteger { get; set; }

        #region Allors
        [Id("c48854d4-6d02-4013-b69c-167ec4df7253")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        I2[] SessionI2DatabaseI2One2Manies { get; set; }

        #region Allors
        [Id("79f730a5-87bb-4d88-a91d-dbef4d39a152")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C1[] SessionI2DatabaseC1Many2Manies { get; set; }

        #region Allors
        [Id("5267602e-04d5-45f2-84bd-d57d64b138d8")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        C2 SessionI2DatabaseC2One2One { get; set; }

        #region Allors
        [Id("f76546c5-a05e-4c0d-a1b9-a6cd6a2e8a54")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        double DatabaseI2AllorsDouble { get; set; }

        #endregion

    }
}
