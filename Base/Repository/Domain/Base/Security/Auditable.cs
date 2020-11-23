// <copyright file="Auditable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("6C726DED-C081-46D7-8DCF-F0A376943531")]
    #endregion
    public partial interface Auditable : Object
    {
        #region Allors
        [Id("4BD26F4D-E85B-415A-B956-3FCBE15D4F58")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        User CreatedBy { get; set; }

        #region Allors
        [Id("471CCC05-A48D-47A0-934B-0DD4F8E40C65")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        User LastModifiedBy { get; set; }

        #region Allors
        [Id("C1BA5015-21DD-49A9-AE0F-E70F4035CCA6")]
        #endregion
        [Workspace(Default)]
        DateTime CreationDate { get; set; }

        #region Allors
        [Id("94EB2712-25E1-415B-9657-2DFD460B7969")]
        #endregion
        [Workspace(Default)]
        DateTime LastModifiedDate { get; set; }
    }
}
