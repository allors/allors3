// <copyright file="Singleton.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    public partial class Singleton : Auditable
    {
        #region inherited properties
        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region Allors
        [Id("D53B80EE-468D-463C-8BBF-725105DBA376")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public Settings Settings { get; set; }

        #region Allors
        [Id("076E1D78-8C6A-4A9D-A023-106D3EFB3B87")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public Template NonUnifiedPartBarcodePrintTemplate { get; set; }

        #region Allors
        [Id("CDB21C6E-CEE4-4E2B-839E-CA2F414B4EF9")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToOne)]
        public NonUnifiedPartBarcodePrint NonUnifiedPartBarcodePrint { get; set; }

        #region inherited methods

        #endregion
    }
}
