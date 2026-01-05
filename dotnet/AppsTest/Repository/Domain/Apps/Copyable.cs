// <copyright file="Deletable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("1a2166bb-347b-432b-a092-f6214f726c7e")]
    #endregion
    public partial interface Copyable : Object
    {
        #region Allors
        [Id("5a024e40-df5e-4a87-90e9-b33c744888b7")]
        #endregion
        [Workspace(Default)]
        void Copy();
    }
}
