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
    [Id("b45ec13c-704f-413d-a662-bdc59a17bfe3")]
    #endregion
    public partial interface I12 : S12
    {
        #region Allors
        [Id("042d1311-1c06-4d7c-b68e-eb734f9c7327")]
        [Size(-1)]
        [Workspace(Default)]
        #endregion
        byte[] I12AllorsBinary { get; set; }

        #region Allors
        [Id("107c212d-cc1c-41b2-9c1d-b40c0102072c")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2 I12C2One2One { get; set; }

        #region Allors
        [Id("1611cb5d-4676-4e85-bfc5-5572e8ff1138")]
        [Workspace(Default)]
        #endregion
        double I12AllorsDouble { get; set; }

        #region Allors
        [Id("167b53c0-644c-467e-9f7c-fcb9415d02c6")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1 I12I1Many2One { get; set; }

        #region Allors
        [Id("199a84c4-c7cb-4f23-8b6c-078b14525e18")]
        [Size(256)]
        [Workspace(Default)]
        #endregion
        string I12AllorsString { get; set; }

        #region Allors
        [Id("1bf2abe0-9273-4fb9-b491-020320f1f8db")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12[] I12I12Many2Manies { get; set; }

        #region Allors
        [Id("41a74fec-cfbc-43ca-a6e7-890f0dd1eddb")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        #endregion
        decimal I12AllorsDecimal { get; set; }

        #region Allors
        [Id("4a2b2f43-037d-4149-8a1e-401e5df963ba")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2[] I12I2Many2Manies { get; set; }

        #region Allors
        [Id("51ebb024-c847-4165-b216-b3b6e8883961")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2[] I12C2Many2Manies { get; set; }

        #region Allors
        [Id("59ae05e3-573c-4ea4-9181-2c545236ed1e")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1[] I12I1Many2Manies { get; set; }

        #region Allors
        [Id("5e473f63-b1d7-4530-b64f-26435fb5063c")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12[] I12I12One2Manies { get; set; }

        #region Allors
        [Id("6daafb16-1bc3-4f15-8e25-1a982c5bb3c5")]
        [Size(256)]
        [Workspace(Default)]
        #endregion
        string Name { get; set; }

        #region Allors
        [Id("24B8155F-2741-4742-95F7-49F9B66C2465")]
        [Workspace(Default)]
        #endregion
        int Order { get; set; }

        #region Allors
        [Id("7827af95-147f-4803-865a-b418d567da68")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1[] I12C1Many2Manies { get; set; }

        #region Allors
        [Id("7f6fdb73-3e19-40e7-8feb-6ddbdf2e745a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2 I12I2Many2One { get; set; }

        #region Allors
        [Id("93a59d0a-278d-435b-967e-551523f0cb85")]
        [Workspace(Default)]
        #endregion
        Guid I12AllorsUnique { get; set; }

        #region Allors
        [Id("95551e3a-bad2-4136-923f-c8e5f0f2aec7")]
        [Workspace(Default)]
        #endregion
        int I12AllorsInteger { get; set; }

        #region Allors
        [Id("95c77a0f-7f4c-4142-a93f-f688cfd554af")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1[] I12I1One2Manies { get; set; }

        #region Allors
        [Id("9aefdda0-e547-4c9b-bf28-431669f8ea2e")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1 I12C1One2One { get; set; }

        #region Allors
        [Id("a89b4c06-bba5-4b05-bd6f-c32bc195c32f")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12 I12I12One2One { get; set; }

        #region Allors
        [Id("ac920d1d-290b-484b-9283-3829337182bc")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2 I12I2One2One { get; set; }

        #region Allors
        [Id("b2e3ddda-0cc3-4cfd-a114-9040882ec58a")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12[] Dependencies { get; set; }

        #region Allors
        [Id("b2f568a1-51ba-4b6b-a1f1-b82bdec382b5")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2[] I12I2One2Manies { get; set; }

        #region Allors
        [Id("c018face-b292-455c-a2c0-8f71377fb6cb")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2 I12C2Many2One { get; set; }

        #region Allors
        [Id("c6ecc142-0fbd-48b7-98ae-994fa9b5b814")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12 I12I12Many2One { get; set; }

        #region Allors
        [Id("ccdd1ae2-263e-4221-9841-4cff1907ee8d")]
        [Workspace(Default)]
        #endregion
        bool I12AllorsBoolean { get; set; }

        #region Allors
        [Id("ce0f7d58-b415-43f3-989b-9d8b34754e4b")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1 I12I1One2One { get; set; }

        #region Allors
        [Id("f302dd07-1abc-409e-aa71-ec9f7ac439aa")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1[] I12C1One2Manies { get; set; }

        #region Allors
        [Id("f6436bc9-e307-4001-8f1f-5b37553ab3c6")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1 I12C1Many2One { get; set; }

        #region Allors
        [Id("fa6656dc-3a7a-4701-bc6b-3cd06aaa4483")]
        [Workspace(Default)]
        #endregion
        DateTime I12AllorsDateTime { get; set; }
    }
}
