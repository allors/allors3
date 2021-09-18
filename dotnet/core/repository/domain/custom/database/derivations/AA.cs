// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("F911BF31-57C6-4E00-A1F3-E4711B3F6CFD")]
    #endregion
    public partial class AA : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("44BB5A2A-5C7F-49EE-84DA-891E61DF0ED2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public BB One2One { get; set; }

        #region Allors
        [Id("f59827bd-018c-4114-8048-828c40d919ff")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public BB UnusedOne2One { get; set; }

        #region Allors
        [Id("bda57acf-0da7-4541-a339-93490e53f5c7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public BB Many2One { get; set; }

        #region Allors
        [Id("27f50f3d-29b1-43ca-9531-f344f709988e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public BB UnusedMany2One { get; set; }

        #region Allors
        [Id("72347565-c1e6-4da1-905d-72f1b5322c70")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public BB[] One2Many { get; set; }

        #region Allors
        [Id("b9c68281-12a3-49c8-a7ab-01a6ff2ab92b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public BB[] UnusedOne2Many { get; set; }

        #region Allors
        [Id("20d0d398-56c9-415f-84e2-67899025a80e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public BB[] Many2Many { get; set; }

        #region Allors
        [Id("f3b36b5e-c560-43f7-a1d4-bce1918710b3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public BB[] UnusedMany2Many { get; set; }

        #region Allors
        [Id("ff5b1760-b38a-4f17-91d5-ffa8d71d74bd")]
        #endregion
        [Derived]
        public bool IsCreated { get; set; }

        #region Allors
        [Id("3CC45EC4-70C8-4828-A2F2-DFEDAEF01354")]
        #endregion
        [Size(256)]
        public string Assigned { get; set; }

        #region Allors
        [Id("88C2E7B9-3B7F-468D-BB4E-ACCA1F4365FE")]
        #endregion
        [Size(256)]
        [Derived]
        public string Derived { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
