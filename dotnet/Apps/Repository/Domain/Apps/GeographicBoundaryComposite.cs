// <copyright file="GeographicBoundaryComposite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("3b7ac95a-fdab-488d-b599-17ef9fcf33b0")]
    #endregion
    public partial interface GeographicBoundaryComposite : GeographicBoundary
    {
        #region Allors
        [Id("77d5f129-6096-45da-8b9f-39ef19276f1d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]

        GeographicBoundary[] Associations { get; set; }
    }
}
