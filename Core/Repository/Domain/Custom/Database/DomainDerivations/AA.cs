// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("F911BF31-57C6-4E00-A1F3-E4711B3F6CFD")]
    #endregion
    public partial class AA : Object
    {
        #region inherited properties
   
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("44BB5A2A-5C7F-49EE-84DA-891E61DF0ED2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public BB One2One { get; set; }

        #region Allors
        [Id("88C2E7B9-3B7F-468D-BB4E-ACCA1F4365FE")]
        #endregion
        [Size(256)]
        public string Derived { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
