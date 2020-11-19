// <copyright file="Version.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("A6A3C79E-150B-4586-96EA-5AC0E2E638C6")]
    #endregion
    public partial interface Version : Object
    {
        #region Allors
        [Id("9FAEB940-A3A0-4E7A-B889-BCFD92F6A882")]
        #endregion
        Guid DerivationId { get; set; }

        #region Allors
        [Id("ADF611C3-047A-4BAE-95E3-776022D5CE7B")]
        #endregion
        [Workspace(Default)]
        DateTime DerivationTimeStamp { get; set; }
    }
}
