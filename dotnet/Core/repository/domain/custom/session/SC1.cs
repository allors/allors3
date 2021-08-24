// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("65e0ac1c-a1bc-4ca6-be5f-5e2a5e4bf9f0")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Session)]
    public partial class SC1 : SI1, DerivationCounted, Object
    {
        #region inherited properties
        public SI1 SI1SI1Many2One { get; set; }

        public SI12[] SI1SI12Many2Manies { get; set; }

        public SI2[] SI1SI2Many2Manies { get; set; }

        public SI2 SI1SI2Many2One { get; set; }

        public string SI1AllorsString { get; set; }

        public SI12 SI1SI12Many2One { get; set; }

        public DateTime SI1AllorsDateTime { get; set; }

        public SI2[] SI1SI2One2Manies { get; set; }

        public SC2[] SI1SC2One2Manies { get; set; }

        public SC1 SI1SC1One2One { get; set; }

        public int SI1AllorsInteger { get; set; }

        public SC2[] SI1SC2Many2Manies { get; set; }

        public SI1[] SI1SI1One2Manies { get; set; }

        public SI1[] SI1SI1Many2Manies { get; set; }

        public bool SI1AllorsBoolean { get; set; }

        public decimal SI1AllorsDecimal { get; set; }

        public SI12 SI1SI12One2One { get; set; }

        public SI2 SI1SI2One2One { get; set; }

        public SC2 SI1SC2One2One { get; set; }

        public SC1[] SI1SC1One2Manies { get; set; }

        public byte[] SI1AllorsBinary { get; set; }

        public SC1[] SI1SC1Many2Manies { get; set; }

        public double SI1AllorsDouble { get; set; }

        public SI1 SI1SI1One2One { get; set; }

        public SC1 SI1SC1Many2One { get; set; }

        public SI12[] SI1SI12One2Manies { get; set; }

        public SC2 SI1SC2Many2One { get; set; }

        public Guid SI1AllorsUnique { get; set; }

        public byte[] SI12AllorsBinary { get; set; }

        public SC2 SI12SC2One2One { get; set; }

        public double SI12AllorsDouble { get; set; }

        public SI1 SI12SI1Many2One { get; set; }

        public string SI12AllorsString { get; set; }

        public SI12[] SI12SI12Many2Manies { get; set; }

        public decimal SI12AllorsDecimal { get; set; }

        public SI2[] SI12SI2Many2Manies { get; set; }

        public SC2[] SI12SC2Many2Manies { get; set; }

        public SI1[] SI12SI1Many2Manies { get; set; }

        public SI12[] SI12SI12One2Manies { get; set; }

        public string SessionName { get; set; }

        public SC1[] SI12SC1Many2Manies { get; set; }

        public SI2 SI12SI2Many2One { get; set; }

        public Guid SI12AllorsUnique { get; set; }

        public int SI12AllorsInteger { get; set; }

        public SI1[] SI12SI1One2Manies { get; set; }

        public SC1 SI12SC1One2One { get; set; }

        public SI12 SI12SI12One2One { get; set; }

        public SI2 SI12SI2One2One { get; set; }

        public SI12[] SessionDependencies { get; set; }

        public SI2[] SI12SI2One2Manies { get; set; }

        public SC2 SI12SC2Many2One { get; set; }

        public SI12 SI12SI12Many2One { get; set; }

        public bool SI12AllorsBoolean { get; set; }

        public SI1 SI12SI1One2One { get; set; }

        public SC1[] SI12SC1One2Manies { get; set; }

        public SC1 SI12SC1Many2One { get; set; }

        public DateTime SI12AllorsDateTime { get; set; }

        public WI1 SI1WI1Many2One { get; set; }

        public WI12[] SI1WI12Many2Manies { get; set; }

        public WI2[] SI1WI2Many2Manies { get; set; }

        public WI2 SI1WI2Many2One { get; set; }

        public string WI1AllorsString { get; set; }

        public WI12 SI1WI12Many2One { get; set; }

        public DateTime WI1AllorsDateTime { get; set; }

        public WI2[] SI1WI2One2Manies { get; set; }

        public WC2[] SI1WC2One2Manies { get; set; }

        public WC1 SI1WC1One2One { get; set; }

        public int WI1AllorsInteger { get; set; }

        public WC2[] SI1WC2Many2Manies { get; set; }

        public WI1[] SI1WI1One2Manies { get; set; }

        public WI1[] SI1WI1Many2Manies { get; set; }

        public bool WI1AllorsBoolean { get; set; }

        public decimal WI1AllorsDecimal { get; set; }

        public WI12 SI1WI12One2One { get; set; }

        public WI2 SI1WI2One2One { get; set; }

        public WC2 SI1WC2One2One { get; set; }

        public WC1[] SI1WC1One2Manies { get; set; }

        public byte[] WI1AllorsBinary { get; set; }

        public WC1[] SI1WC1Many2Manies { get; set; }

        public double WI1AllorsDouble { get; set; }

        public WI1 SI1WI1One2One { get; set; }

        public WC1 SI1WC1Many2One { get; set; }

        public WI12[] SI1WI12One2Manies { get; set; }

        public WC2 SI1WC2Many2One { get; set; }

        public Guid WI1AllorsUnique { get; set; }

        public byte[] WI12AllorsBinary { get; set; }

        public WC2 SI12WC2One2One { get; set; }

        public double WI12AllorsDouble { get; set; }

        public WI1 SI12WI1Many2One { get; set; }

        public string WI12AllorsString { get; set; }

        public WI12[] SI12WI12Many2Manies { get; set; }

        public decimal WI12AllorsDecimal { get; set; }

        public WI2[] SI12WI2Many2Manies { get; set; }

        public WC2[] SI12WC2Many2Manies { get; set; }

        public WI1[] SI12WI1Many2Manies { get; set; }

        public WI12[] SI12WI12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public WC1[] SI12WC1Many2Manies { get; set; }

        public WI2 SI12WI2Many2One { get; set; }

        public Guid WI12AllorsUnique { get; set; }

        public int WI12AllorsInteger { get; set; }

        public WI1[] SI12WI1One2Manies { get; set; }

        public WC1 SI12WC1One2One { get; set; }

        public WI12 SI12WI12One2One { get; set; }

        public WI2 SI12WI2One2One { get; set; }

        public WI12[] WorkspaceDependencies { get; set; }

        public WI2[] SI12WI2One2Manies { get; set; }

        public WC2 SI12WC2Many2One { get; set; }

        public WI12 SI12WI12Many2One { get; set; }

        public bool WI12AllorsBoolean { get; set; }

        public WI1 SI12WI1One2One { get; set; }

        public WC1[] SI12WC1One2Manies { get; set; }

        public WC1 SI12WC1Many2One { get; set; }

        public DateTime WI12AllorsDateTime { get; set; }

        public I1 SI1DatabaseI1Many2One { get; set; }

        public I12[] SI1DatabaseI12Many2Manies { get; set; }

        public I2[] SI1DatabaseI2Many2Manies { get; set; }

        public I2 SI1DatabaseI2Many2One { get; set; }

        public string DatabaseI1AllorsString { get; set; }

        public I12 SI1DatabaseI12Many2One { get; set; }

        public DateTime DatabaseI1AllorsDateTime { get; set; }

        public I2[] SI1DatabaseI2One2Manies { get; set; }

        public C2[] SI1DatabaseC2One2Manies { get; set; }

        public C1 SI1DatabaseC1One2One { get; set; }

        public int DatabaseI1AllorsInteger { get; set; }

        public C2[] SI1DatabaseC2Many2Manies { get; set; }

        public I1[] SI1DatabaseI1One2Manies { get; set; }

        public I1[] SI1DatabaseI1Many2Manies { get; set; }

        public bool DatabaseI1AllorsBoolean { get; set; }

        public decimal DatabaseI1AllorsDecimal { get; set; }

        public I12 SI1DatabaseI12One2One { get; set; }

        public I2 SI1DatabaseI2One2One { get; set; }

        public C2 SI1DatabaseC2One2One { get; set; }

        public C1[] SI1DatabaseC1One2Manies { get; set; }

        public byte[] DatabaseI1AllorsBinary { get; set; }

        public C1[] SI1DatabaseC1Many2Manies { get; set; }

        public double DatabaseI1AllorsDouble { get; set; }

        public I1 SI1DatabaseI1One2One { get; set; }

        public C1 SI1DatabaseC1Many2One { get; set; }

        public I12[] SI1DatabaseI12One2Manies { get; set; }

        public C2 SI1DatabaseC2Many2One { get; set; }

        public Guid DatabaseI1AllorsUnique { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 SI12DatabaseC2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 SI12DatabaseI1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] SI12DatabaseI12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] SI12DatabaseI2Many2Manies { get; set; }

        public C2[] SI12DatabaseC2Many2Manies { get; set; }

        public I1[] SI12DatabaseI1Many2Manies { get; set; }

        public I12[] SI12DatabaseI12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] SI12DatabaseC1Many2Manies { get; set; }

        public I2 SI12DatabaseI2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] SI12DatabaseI1One2Manies { get; set; }

        public C1 SI12DatabaseC1One2One { get; set; }

        public I12 SI12DatabaseI12One2One { get; set; }

        public I2 SI12DatabaseI2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] SI12DatabaseI2One2Manies { get; set; }

        public C2 SI12DatabaseC2Many2One { get; set; }

        public I12 SI12DatabaseI12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 SI12DatabaseI1One2One { get; set; }

        public C1[] SI12DatabaseC1One2Manies { get; set; }

        public C1 SI12DatabaseC1Many2One { get; set; }

        public DateTime DatabaseI12AllorsDateTime { get; set; }


        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public int DerivationCount { get; set; }

        public bool ChangedRolePingS12 { get; set; }
        public bool ChangedRolePongS12 { get; set; }
        public bool ChangedRolePingI12 { get; set; }
        public bool ChangedRolePongI12 { get; set; }
        public bool ChangedRolePingI1 { get; set; }
        public bool ChangedRolePongI1 { get; set; }
        public bool ChangedRolePingC1 { get; set; }
        public bool ChangedRolePongC1 { get; set; }


        #endregion

        #region Session Relation
        #region Unit Role
        #region Allors
        [Id("EC5A57B8-411E-46E2-9FC5-49AC13A6A6B0")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public byte[] SessionAllorsBinary { get; set; }

        #region Allors
        [Id("D6DF97A1-5AFF-4076-880D-662BC0E4B776")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public bool SessionAllorsBoolean { get; set; }

        #region Allors
        [Id("7F762815-ADA7-4824-BD6F-07097B4C9BC2")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionAllorsDateTime { get; set; }

        #region Allors
        [Id("D8F257B5-EAED-4FB7-9967-DFA0E8755F2B")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeLessThan { get; set; }

        #region Allors
        [Id("60045BED-1CEA-4C9B-9069-DE31394CD58F")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeGreaterThan { get; set; }

        #region Allors
        [Id("EEF3A4B0-0243-4453-9886-B398F5B3CBFA")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenA { get; set; }

        #region Allors
        [Id("3E7CBF54-C719-4FEC-9825-A7B33A917C82")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public DateTime SessionDateTimeBetweenB { get; set; }

        #region Allors
        [Id("0FDAEDB4-A89D-4CFB-BAD6-964ED6432749")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public decimal SessionAllorsDecimal { get; set; }

        #region Allors
        [Id("AFA006C8-CDBC-4C48-B8F0-BC5185D42213")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalLessThan { get; set; }

        #region Allors
        [Id("5601D39D-8A3C-494B-ACCC-5DAD8E4C9C41")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalGreaterThan { get; set; }

        #region Allors
        [Id("4916E256-2832-4EF8-9F29-2BB9F523D5A6")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenA { get; set; }

        #region Allors
        [Id("F7D443FD-A689-47B1-9CD3-71A539D0478E")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Decimal SessionDecimalBetweenB { get; set; }

        #region Allors
        [Id("416F3732-1747-43EE-96EF-C4178F6D0FBB")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public double SessionAllorsDouble { get; set; }

        #region Allors
        [Id("2A51C97B-F915-4411-8B8D-9ED55AA87961")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleLessThan { get; set; }

        #region Allors
        [Id("9258BF30-D51C-4232-AC66-66B077B8566C")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleGreaterThan { get; set; }

        #region Allors
        [Id("6C323869-186F-42C4-8875-66CFFE5914D1")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenA { get; set; }

        #region Allors
        [Id("58B76AF4-E469-4111-8489-E6C0CB836126")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Double SessionDoubleBetweenB { get; set; }

        #region Allors
        [Id("35BC4214-4DD0-4C42-A157-6FA9324104FD")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionAllorsInteger { get; set; }

        #region Allors
        [Id("59468CA8-C933-461B-9034-89AE4008DA5C")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerLessThan { get; set; }

        #region Allors
        [Id("0A7F2011-2752-4527-A938-54C945AE1D65")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerGreaterThan { get; set; }

        #region Allors
        [Id("7903BBB5-DD1E-4D0A-8324-DA42C60E4164")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenA { get; set; }

        #region Allors
        [Id("C65323A0-AF08-48CB-9EB7-8782E99DC461")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public int SessionIntegerBetweenB { get; set; }

        #region Allors
        [Id("80181D9C-87CA-46C3-A79A-B102BF8F6C5C")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsString { get; set; }

        #region Allors
        [Id("CD97EBE6-3141-4D7F-B993-51DC867649A0")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringEquals { get; set; }

        #region Allors
        [Id("43D124A4-2E69-4ACE-B80D-F2675D2CCF4B")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string SessionAllorsStringMax { get; set; }

        #region Allors
        [Id("6A689D92-C119-42FF-AE00-BCA02ACC6B34")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public Guid SessionAllorsUnique { get; set; }
        #endregion

        #region Session Role
        #region Allors
        [Id("82EE012B-9A63-40FB-89BB-22BE7CC5FCB1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1Many2Manies { get; set; }

        #region Allors
        [Id("FF4358A5-72F4-4807-87FB-C057535FD026")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1Many2One { get; set; }

        #region Allors
        [Id("E7EE1AD8-78D9-446E-B105-7726D0E400C4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1[] SessionSC1One2Manies { get; set; }

        #region Allors
        [Id("E0D2EDF6-730C-4DF3-888E-B3BD966E6C3A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC1 SessionSC1One2One { get; set; }

        #region Allors
        [Id("EC7C8050-5C21-41DD-91C7-D24F33427301")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2Many2Manies { get; set; }

        #region Allors
        [Id("25E3E786-3692-4F21-8FF5-AB3DA3805D7C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2Many2One { get; set; }

        #region Allors
        [Id("A4F1F557-8BDE-4FC4-BEB8-56096BC1CA4B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2[] SessionSC2One2Manies { get; set; }

        #region Allors
        [Id("14F05AA8-B307-453E-8BCF-A6FCECCF4B81")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SC2 SessionSC2One2One { get; set; }

        #region Allors
        [Id("397BBCA7-4CC6-41F8-B243-9F3FB12A3DC7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12Many2Manies { get; set; }

        #region Allors
        [Id("D8DDEEB4-74DC-47F0-A493-91411A17B68D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12Many2One { get; set; }

        #region Allors
        [Id("64D5F54A-0DBB-4984-BFAB-B5772AFAA81E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12[] SessionSI12One2Manies { get; set; }

        #region Allors
        [Id("34E68C58-7030-49CE-8C72-6B7BB62743EE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI12 SessionSI12One2One { get; set; }

        #region Allors
        [Id("33BC7407-1A68-4CDA-8CED-302F54807322")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1Many2Manies { get; set; }

        #region Allors
        [Id("F5DA2CD8-75F5-4826-A62D-E7865B8A6FE5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1Many2One { get; set; }

        #region Allors
        [Id("814DD65D-A4ED-49B7-83AE-1FB6564B42FE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1[] SessionSI1One2Manies { get; set; }

        #region Allors
        [Id("9AD37328-91B4-4CE0-BE00-63603C78B78B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI1 SessionSI1One2One { get; set; }

        #region Allors
        [Id("C8B6376F-168F-4845-9672-9C60EDD904DD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2[] SessionSI2Many2Manies { get; set; }

        #region Allors
        [Id("46B853B6-68B0-49F8-80D9-BEC6DD7283D5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2Many2One { get; set; }

        #region Allors
        [Id("C4749F52-5C97-401C-BF43-70093F2D27ED")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public SI2[] SessionSI2One2Manies { get; set; }

        #region Allors
        [Id("6F864539-EF06-4CDE-B40B-4D86243A41F6")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public SI2 SessionSI2One2One { get; set; }
        #endregion

        #region Workspace Role
        #region Allors
        [Id("D17B0544-D814-43CF-AD04-663200B65ECF")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1Many2Manies { get; set; }

        #region Allors
        [Id("2C4529EA-B202-468A-8413-3B00F53AC16D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1Many2One { get; set; }

        #region Allors
        [Id("A699FE92-1AA8-466C-9349-6F1F9F68AF31")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1[] SessionWC1One2Manies { get; set; }

        #region Allors
        [Id("06F5F96F-0EC4-4D8F-9ABA-9DAEF1C7E93B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC1 SessionWC1One2One { get; set; }

        #region Allors
        [Id("B72B2F4D-9D20-4D01-A950-7A2E0E04284E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2Many2Manies { get; set; }

        #region Allors
        [Id("CA28B353-2D47-4ED3-B0AF-FF52D7D7321F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2Many2One { get; set; }

        #region Allors
        [Id("8392F1DB-07B5-4B38-9FDB-A789F0EDBB0C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2[] SessionWC2One2Manies { get; set; }

        #region Allors
        [Id("44FF7A26-0C8B-47EC-8EC0-26CED237ECC1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WC2 SessionWC2One2One { get; set; }

        #region Allors
        [Id("E5BB01FE-6B5E-4628-8798-EFCB97B03530")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12Many2Manies { get; set; }

        #region Allors
        [Id("7F835DF7-9C59-4B1C-8CB4-BF0F4DFE687D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12Many2One { get; set; }

        #region Allors
        [Id("0932C417-27B9-4E68-B694-03E776F263D9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12[] SessionWI12One2Manies { get; set; }

        #region Allors
        [Id("B4F56479-A006-4879-8BB3-C7514847B721")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI12 SessionWI12One2One { get; set; }

        #region Allors
        [Id("D079EDE1-477F-457A-A315-962F845EFB01")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1Many2Manies { get; set; }

        #region Allors
        [Id("46D74AA7-FE9F-4C78-958F-29DAD810A55B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1Many2One { get; set; }

        #region Allors
        [Id("28E33F38-B4F3-46B7-BFF6-7480ED51A7E8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1[] SessionWI1One2Manies { get; set; }

        #region Allors
        [Id("E2819ED0-54C1-41BF-AC56-111E6B1ADAB0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI1 SessionWI1One2One { get; set; }

        #region Allors
        [Id("E49AE44D-C278-4746-88B5-5C60A527CE81")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2[] SessionWI2Many2Manies { get; set; }

        #region Allors
        [Id("233BB1D7-BAF1-46A0-B8C7-2AA518EAD502")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2Many2One { get; set; }

        #region Allors
        [Id("CC1A2A6F-D006-405D-A228-936AAE53A59B")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public WI2[] SessionWI2One2Manies { get; set; }

        #region Allors
        [Id("3AF52AFC-5199-4C9A-B5E9-00C3EE473BB0")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public WI2 SessionWI2One2One { get; set; }
        #endregion

        #region Database Role
        #region Allors
        [Id("E31ED89F-A6AA-487E-ABF0-921516C43B5E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1Many2Manies { get; set; }

        #region Allors
        [Id("D1017F9C-C845-400E-AFB3-AFE92ED8849F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1Many2One { get; set; }

        #region Allors
        [Id("95B15476-7FD7-49B9-AC8A-A799D3D53C67")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1[] SessionC1One2Manies { get; set; }

        #region Allors
        [Id("D4323AC0-2991-4A12-8FA6-2E2FB36A14A0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C1 SessionC1One2One { get; set; }

        #region Allors
        [Id("D7E96FB2-CCF6-4245-9647-BB0C8AA8AB6B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2Many2Manies { get; set; }

        #region Allors
        [Id("7008BAAF-D38E-49F8-8D47-2F45FDF9E565")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 WSessionC2Many2One { get; set; }

        #region Allors
        [Id("DB9F6E00-DF0C-4C94-AA0C-583AF75D8F48")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2[] SessionC2One2Manies { get; set; }

        #region Allors
        [Id("BDABB246-E8B8-482C-88DD-6A301BF15F90")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public C2 SessionC2One2One { get; set; }

        #region Allors
        [Id("769426B4-5533-4C15-9A1B-2B7319D9E66F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12Many2Manies { get; set; }

        #region Allors
        [Id("E97E9D5F-2F35-4F3C-9908-0B09D6429B87")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12Many2One { get; set; }

        #region Allors
        [Id("48310C99-B122-4BCE-ACC4-68F067EFBB58")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12[] SessionI12One2Manies { get; set; }

        #region Allors
        [Id("02755C84-E2E8-47C9-9B81-8E521C944BC0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I12 SessionI12One2One { get; set; }

        #region Allors
        [Id("9B0F19D8-0E4E-486D-8126-D3726E3BBDBA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1Many2Manies { get; set; }

        #region Allors
        [Id("FCCF9FDC-546E-4C90-AAB9-A2FE76F3BCDC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1Many2One { get; set; }

        #region Allors
        [Id("C70F614A-D00C-4687-8E5D-5980FC18D5E7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1[] SessionI1One2Manies { get; set; }

        #region Allors
        [Id("7C3411E3-697E-460B-A7F0-92485A737C6A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I1 SessionI1One2One { get; set; }

        #region Allors
        [Id("5101A20D-F920-4CB3-9E90-32C5C8960E83")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2[] SessionI2Many2Manies { get; set; }

        #region Allors
        [Id("63085391-A39B-4C10-A825-EB1C9CE4DD3E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2Many2One { get; set; }

        #region Allors
        [Id("BAF68ED8-D58E-40B2-9A71-8170D0F2DC66")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Multiplicity(Multiplicity.OneToMany)]
        public I2[] SessionI2One2Manies { get; set; }

        #region Allors
        [Id("865B2A8F-45F0-4913-866B-881F3896A1F5")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public I2 SessionI2One2One { get; set; }
        #endregion
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void InterfaceMethod() { }

        public void SuperinterfaceMethod() { }

        #endregion
    }
}
