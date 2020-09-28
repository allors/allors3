// <copyright file="CityBound.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("bfdd33dc-5701-41ec-a768-f745155663d3")]
    #endregion
    public partial interface CityBound : Object
    {
        #region Allors
        [Id("7723a00d-8764-40e2-99a8-a790401689b5")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        City[] Cities { get; set; }
    }
}
