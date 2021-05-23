// <copyright file="I1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("fefcf1b6-ac8f-47b0-bed5-939207a2833e")]
    #endregion
    public partial interface I1 : Object, S1, S1234
    {
        #region Allors
        [Id("00a70a04-4fc8-4585-83ce-0f7f0e0db7ab")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I34[] I1I34one2manies { get; set; }

        #region Allors
        [Id("036e3008-07f8-4a15-bca2-eb21837778a0")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I2[] I1I2one2manies { get; set; }

        #region Allors
        [Id("0b0f8c40-266c-424a-8276-0e8e2673d1a7")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I2 I1I2many2one { get; set; }

        #region Allors
        [Id("0d63e4c7-28de-4d47-8f23-7ee1d3606751")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C2 I1C2many2one { get; set; }

        #region Allors
        [Id("14a93943-13f6-481d-98c7-19fb55625af9")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C2 I1C2one2one { get; set; }

        #region Allors
        [Id("19e09e31-31ac-44cc-ad1e-a015f4747aeb")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I1DecimalBetweenA { get; set; }

        #region Allors
        [Id("1d41941b-3b1d-48d7-bc6f-e8811cbd96e4")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        S1 I1S1one2one { get; set; }

        #region Allors
        [Id("28b92468-27e5-4471-b3a5-37b8ec4f794e")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I12 I1I12many2one { get; set; }

        #region Allors
        [Id("28ceffc2-c776-4a0a-9825-a6d1bcb265dc")]
        [Size(256)]
        #endregion
        string I1AllorsString { get; set; }

        #region Allors
        [Id("29244f33-6d79-44aa-9ed2-8cc01b5070b7")]
        #endregion
        DateTime I1DateTimeLessThan { get; set; }

        #region Allors
        [Id("2cd562b6-7f54-49af-b853-2244f10ec60e")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C2[] I1C2one2manies { get; set; }

        #region Allors
        [Id("2e98ec7e-486f-4b96-ac15-5149fe6c4e0e")]
        [Size(100000)]
        #endregion
        string I1StringLarge { get; set; }

        #region Allors
        [Id("2f739fa2-c169-4721-8d2d-79f27a6e57c6")]
        #endregion
        double I1FloatLessThan { get; set; }

        #region Allors
        [Id("32fc21cc-4be7-4a0e-ac71-df135be95e68")]
        #endregion
        DateTime I1AllorsDateTime { get; set; }

        #region Allors
        [Id("33f13167-3a14-4b06-a1d8-87076918b285")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C1 I1C1many2one { get; set; }

        #region Allors
        [Id("381c61c1-312d-47ea-8314-8ac051378a81")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        [Indexed]
        I12 I1I12one2one { get; set; }

        #region Allors
        [Id("39f1c13c-7d77-429f-ac9b-1491e949aa3a")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I1DecimalGreaterThan { get; set; }

        #region Allors
        [Id("4401d0b8-2450-45a8-92d2-ff3961e129b2")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C1 I1C1one2one { get; set; }

        #region Allors
        [Id("4a30d40e-ade3-4304-b17b-185abc8b7fde")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I2[] I1I2many2manies { get; set; }

        #region Allors
        [Id("518da995-1f6b-4632-94f1-11cea5e72717")]
        #endregion
        int I1IntegerBetweenA { get; set; }

        #region Allors
        [Id("528ece9c-81f2-4ea4-8d42-50d9a3fe1eea")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I34 I1I34many2one { get; set; }

        #region Allors
        [Id("58d75a73-61d3-4ad7-bd1a-b3e673d8ee31")]
        #endregion
        double I1FloatBetweenA { get; set; }

        #region Allors
        [Id("5901c4d4-420f-47a3-87e3-ac04b4601efc")]
        #endregion
        int I1IntegerLessThan { get; set; }

        #region Allors
        [Id("5cb44331-fd8c-4f73-8994-161f702849b6")]
        #endregion
        int I1AllorsInteger { get; set; }

        #region Allors
        [Id("68549750-b8f9-4a29-a078-803e7348e142")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        S2 I1S2one2one { get; set; }

        #region Allors
        [Id("6c3d04be-6f95-44b8-863a-245e150e3110")]
        #endregion
        bool I1AllorsBoolean { get; set; }

        #region Allors
        [Id("6e7c286c-42e0-45d7-8ad8-ac0ed91dbbb5")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I1 I1I1many2one { get; set; }

        #region Allors
        [Id("7014e84c-62c4-48ba-b4ec-ab52a897f443")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C1[] I1C1many2manies { get; set; }

        #region Allors
        [Id("70312f37-52e9-4cf6-9dd6-b357628ea3ed")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        I2 I1I2one2one { get; set; }

        #region Allors
        [Id("818b4013-5ef1-4455-9f0d-9a39fa3425bb")]
        [Precision(10)]
        [Scale(2)]
        #endregion
        decimal I1AllorsDecimal { get; set; }

        #region Allors
        [Id("82a81e9e-7a13-43d3-bb8f-227edfe26a1f")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        S1[] I1S1many2manies { get; set; }

        #region Allors
        [Id("9095f55b-de23-49d7-a28e-918c22c5cfd2")]
        #endregion
        DateTime I1DateTimeGreaterThan { get; set; }

        #region Allors
        [Id("912eeb1b-c5d6-4ea3-9e66-6d92cc455ef6")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I34[] I1I34many2manies { get; set; }

        #region Allors
        [Id("9291fb85-9d1f-4c5d-96ec-797be51557ce")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        I34 I1I34one2one { get; set; }

        #region Allors
        [Id("95fff847-922f-4d6f-9e98-37013bdf6b06")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I1[] I1I1one2manies { get; set; }

        #region Allors
        [Id("9735d027-4249-4540-9658-f3ec06d3b868")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I1[] I1I1many2manies { get; set; }

        #region Allors
        [Id("973d6e4f-57ff-454a-9621-bd5dccb65525")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        S2[] I1S2many2manies { get; set; }

        #region Allors
        [Id("9b05ecb0-c3d5-4b11-98dc-653aef9f65cc")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I12[] I1I12many2manies { get; set; }

        #region Allors
        [Id("9f70c4eb-2e36-4ae1-8ed2-b3fab908e392")]
        [Size(256)]
        #endregion
        string I1StringEquals { get; set; }

        #region Allors
        [Id("a458ad6e-0f4a-473b-a233-04b8e7fadf62")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I12[] I1I12one2manies { get; set; }

        #region Allors
        [Id("a77bcd80-82df-4b76-a1bc-8e78106d7d53")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        S2[] I1S2one2manies { get; set; }

        #region Allors
        [Id("b4f171d3-1463-41bc-8230-e53e5a717b89")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C2[] I1C2many2manies { get; set; }

        #region Allors
        [Id("b9c67658-4abc-41f3-9434-c8512a482179")]
        [Size(-1)]
        #endregion
        byte[] I1AllorsBinary { get; set; }

        #region Allors
        [Id("c04d1e56-2686-495b-a02d-cda84f7cd2ff")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I1DecimalBetweenB { get; set; }

        #region Allors
        [Id("c3496e43-335b-43b8-9fed-44439c9ae0d1")]
        #endregion
        double I1FloatGreaterThan { get; set; }

        #region Allors
        [Id("c892a286-fe92-4b8b-98ba-c5e02fb96279")]
        #endregion
        int I1IntegerBetweenB { get; set; }

        #region Allors
        [Id("c95ac96b-4385-4e31-8719-f120c76ab67a")]
        #endregion
        DateTime I1DateTimeBetweenA { get; set; }

        #region Allors
        [Id("cdb758bf-ecaf-4d99-88fb-58df9258c13c")]
        #endregion
        double I1AllorsDouble { get; set; }

        #region Allors
        [Id("d24b5b74-6ea2-4788-857c-90e0ba1433a5")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        S1[] I1S1one2manies { get; set; }

        #region Allors
        [Id("ddbfe021-3310-4d8e-a4ef-438306aaf191")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        I1 I1I1one2one { get; set; }

        #region Allors
        [Id("e8f1c37a-6bae-4ff5-b385-39bff287bf78")]
        #endregion
        int I1IntegerGreaterThan { get; set; }

        #region Allors
        [Id("ee44a1bb-a5c7-4b05-a06b-8ff9ca9d4f98")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        S1 I1S1many2one { get; set; }

        #region Allors
        [Id("eec19d8e-727c-437a-95db-b301cd1cd65a")]
        #endregion
        double I1FloatBetweenB { get; set; }

        #region Allors
        [Id("f1a1ef6a-8275-4b57-8cd0-8e79ee5a517d")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I1DecimalLessThan { get; set; }

        #region Allors
        [Id("f5a6b7d9-9f49-44a8-b303-1a2969195bd1")]
        #endregion
        DateTime I1DateTimeBetweenB { get; set; }

        #region Allors
        [Id("f9d7411e-7993-4e43-a7e2-726f1e44e29c")]
        #endregion
        Guid I1AllorsUnique { get; set; }

        #region Allors
        [Id("fbc1fd9f-853a-4b7d-b618-447b765b3bcb")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C1[] I1C1one2manies { get; set; }

        #region Allors
        [Id("fe51c02e-ed28-4628-9da1-7bc2131c8992")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        S2 I1S2many2one { get; set; }
    }
}
