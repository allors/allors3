// <copyright file="Commentable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("fdd52472-e863-4e91-bb01-1dada2acc8f6")]
    #endregion
    public partial interface Commentable : Object
    {
        #region Allors
        [Id("d800f9a2-fadd-45f1-8731-4dac177c6b1b")]
        #endregion
        [Size(-1)]
        [Workspace]
        [MediaType("text/markdown")]
        string Comment { get; set; }

        #region Allors
        [Id("88CDC491-70DB-4B5C-B628-B18806D1FDBD")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedText[] LocalisedComments { get; set; }
    }
}
