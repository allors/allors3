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
    [Id("2F34A198-F93A-4AC7-A570-EF84041421EA")]
    #endregion
    [Workspace(Default)]
    public partial class TraceY : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Unit
        #region Allors
        [Id("4FB352CE-3853-4F0C-A592-476D5D31F3B2")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public byte[] AllorsBinary { get; set; }

        #region Allors
        [Id("45176DB1-0D4C-471F-8696-7E1B5586A593")]
        #endregion
        [Workspace(Default)]
        public bool AllorsBoolean { get; set; }

        #region Allors
        [Id("C316C945-1ECB-4C71-95AB-B7B48B91A4EE")]
        #endregion
        [Workspace(Default)]
        public DateTime AllorsDateTime { get; set; }

        #region Allors
        [Id("E01F079B-08B3-43EB-87B8-6E12F5B0EBC0")]
        #endregion
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AllorsDecimal { get; set; }

        #region Allors
        [Id("6D1B30BB-5B0D-4AC5-97CB-5C21410C9C29")]
        #endregion
        [Workspace(Default)]
        public double AllorsDouble { get; set; }

        #region Allors
        [Id("71EACB19-7DFD-4CB0-9D72-C7EBC9307289")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public int AllorsInteger { get; set; }

        #region Allors
        [Id("38524E13-085D-48A9-A1C0-435800072FAF")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string AllorsString { get; set; }

        #region Allors
        [Id("898B541C-E145-4232-8802-77BEFE6A5439")]
        #endregion
        [Workspace(Default)]
        public Guid AllorsUnique { get; set; }
        #endregion

        #region Composite
        #region Allors
        [Id("C88D1B2C-C13F-4CB4-B2C4-ABAD5D9A0A42")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public TraceZ[] Many2Manies { get; set; }

        #region Allors
        [Id("0487AA91-D680-44D8-A47D-F400A47113F2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public TraceZ Many2One { get; set; }

        #region Allors
        [Id("E46C0726-934A-4CD1-8C5D-B36CAE404CAB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public TraceZ[] One2Manies { get; set; }

        #region Allors
        [Id("8AC4F48E-1B52-4208-8CD0-A9EC74C79AF4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public TraceZ One2One { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
