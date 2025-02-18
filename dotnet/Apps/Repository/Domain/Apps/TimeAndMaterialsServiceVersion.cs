// <copyright file="TimeAndMaterialsServiceVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("79b062e8-b42f-490a-ad62-98ea24fba512")]
    #endregion
    public partial class TimeAndMaterialsServiceVersion : Version
    {
        #region inherited properties

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("b9e06065-346d-41f8-ac20-0c73dcee5f33")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]

        [Workspace(Default)]
        public Product[] Variants { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
