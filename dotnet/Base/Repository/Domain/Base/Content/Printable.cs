// <copyright file="Printable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("61207a42-3199-4249-baa4-9dd11dc0f5b1")]
    #endregion
    public partial interface Printable : Object
    {
        #region Allors
        [Id("079C31BA-0D20-4CD7-921C-A1829E226970")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        PrintDocument PrintDocument { get; set; }

        #region Allors
        [Id("55903F87-8D6B-4D99-9E0D-C3B74064C81F")]
        #endregion
        [Workspace(Default)]
        void Print();
    }
}
