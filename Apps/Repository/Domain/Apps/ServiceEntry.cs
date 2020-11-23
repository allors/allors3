// <copyright file="ServiceEntry.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("4a4a0548-b75f-4a79-89aa-f5c242121f11")]
    #endregion
    [Plural("ServiceEntries")]
    public partial interface ServiceEntry : Commentable, Period, Deletable, Object
    {
        #region Allors
        [Id("74fc8f9b-62f3-4921-bce1-ca10eed33204")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        EngagementItem EngagementItem { get; set; }

        #region Allors
        [Id("9b04b715-376f-4c39-b78b-f92af6b4ffc1")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool IsBillable { get; set; }

        #region Allors
        [Id("a6ae42bd-babf-44e1-bdc0-cc403e56e43e")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("b9bb6409-c6b9-4a4b-9d46-02c62b4b3304")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("6c8b3267-4896-467b-80ad-07153adbf704")]
        #endregion
        [Required]
        [Workspace(Default)]
        Guid DerivationTrigger { get; set; }
    }
}
