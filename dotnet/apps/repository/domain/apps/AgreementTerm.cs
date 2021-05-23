// <copyright file="AgreementTerm.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("734be1c9-e6af-49b7-8fe8-331cd7036e2e")]
    #endregion
    public partial interface AgreementTerm : Deletable, Object
    {
        #region Allors
        [Id("85cd1bbd-f2ad-454f-8f04-cdea48ce6196")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string TermValue { get; set; }

        #region Allors
        [Id("b38a35f7-3bf5-4c9c-b2ea-6b220de43e20")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        TermType TermType { get; set; }

        #region Allors
        [Id("d9a68cc0-8fea-4610-9853-f1fca33cbc9a")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }
    }
}
