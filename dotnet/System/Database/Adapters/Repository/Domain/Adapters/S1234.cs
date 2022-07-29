// <copyright file="S1234.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("c3c0ecf3-9f8d-4701-854f-8ddea1bd69fd")]
    #endregion
    public partial interface S1234 : Object
    {
        #region Allors
        [Id("493D7A12-B7E2-455C-AA1E-B4F98C17DD19")]
        #endregion
        [Size(256)]
        string Name { get; set; }

        #region Allors
        [Id("012a43d3-e1e0-4693-a771-1526c29b7ac4")]
        #endregion
        double S1234AllorsDouble { get; set; }

        #region Allors
        [Id("2ac36edd-d718-4252-b7cf-74849e1fca6e")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal S1234AllorsDecimal { get; set; }

        #region Allors
        [Id("46263379-afd4-4472-bb05-057fb88163ab")]
        #endregion
        int S1234AllorsInteger { get; set; }

        #region Allors
        [Id("4b846355-000b-4651-bff2-51f1275c1461")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        S1234 S1234many2one { get; set; }

        #region Allors
        [Id("58a56dee-c613-4d76-ab99-5608e7709cd8")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C2 S1234C2one2one { get; set; }

        #region Allors
        [Id("73302b50-8526-40ae-a202-5b17e1093629")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C2[] S1234C2many2manies { get; set; }

        #region Allors
        [Id("8fb24e1c-9e04-4b3d-8a97-153d3c0ea7ec")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        S1234[] S1234one2manies { get; set; }

        #region Allors
        [Id("94a49847-273f-4e9b-b07b-d615d994757a")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C2[] S1234C2one2manies { get; set; }

        #region Allors
        [Id("a2e7c6f6-ca0d-4fb3-9431-8dd1be7ebdb7")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        S1234[] S1234many2manies { get; set; }

        #region Allors
        [Id("b299db28-1107-4120-946c-fbdad2271c5c")]
        [Size(256)]
        #endregion
        string ClassName { get; set; }

        #region Allors
        [Id("c13e8484-75a3-40be-afd5-44a31aca3771")]
        #endregion
        DateTime S1234AllorsDateTime { get; set; }

        #region Allors
        [Id("c2fac2fc-14c6-4aa3-89ff-afba1316d06d")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        S1234 S1234one2one { get; set; }

        #region Allors
        [Id("df9eb36a-366f-4a5a-a750-f2f23f681c74")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C2 S1234C2many2one { get; set; }

        #region Allors
        [Id("e6164217-2f54-4134-8c53-4a45caa9dd11")]
        [Size(256)]
        #endregion
        string S1234AllorsString { get; set; }

        #region Allors
        [Id("ef45cd72-2e16-47df-b949-c803a554b307")]
        #endregion
        bool S1234AllorsBoolean { get; set; }
    }
}
