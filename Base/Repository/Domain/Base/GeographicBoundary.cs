// <copyright file="GeographicBoundary.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("3453c2e1-77a4-4fe8-b663-02bac689883a")]
    #endregion
    public partial interface GeographicBoundary : GeoLocatable, Object
    {
        #region Allors
        [Id("28e43fe9-cdf1-4671-af95-ead40ecbef15")]
        #endregion
        [Size(10)]

        string Abbreviation { get; set; }
    }
}
