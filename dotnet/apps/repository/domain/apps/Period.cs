// <copyright file="Period.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("80adbbfd-952e-46f3-a744-78e0ce42bc80")]
    #endregion
    public partial interface Period : Object
    {
        #region Allors
        [Id("5aeb31c7-03d4-4314-bbb2-fca5704b1eab")]
        #endregion
        [Required]
        [Workspace(Default)]
        DateTime FromDate { get; set; }

        #region Allors
        [Id("d7576ce2-da27-487a-86aa-b0912f745bc0")]
        #endregion
        [Workspace(Default)]
        DateTime ThroughDate { get; set; }
    }
}
