// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("A32F39D4-2A0F-4270-9CE9-0FADB1121113")]
    #endregion
    [Workspace(Default)]
    public partial class TrimFrom : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("F5EEBEF2-A317-4A4A-9D51-3B00FCFBF7F9")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("11DD2A3B-5C61-4E95-93DD-F1B8BDB14EB1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public TrimTo[] Many2Manies { get; set; }

        #region Allors
        [Id("0F01CBAE-6991-4F5E-B788-C8F4AB799D91")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public TrimTo Many2One { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
