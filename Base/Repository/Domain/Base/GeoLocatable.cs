// <copyright file="GeoLocatable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("93960be2-f676-4e7f-9efb-f99c92303059")]
    #endregion
    public partial interface GeoLocatable : UniquelyIdentifiable, Object
    {
        #region Allors
        [Id("b0aba482-63eb-4482-a232-3863f089f4d9")]
        #endregion
        [Required]
        [Precision(8)]
        [Scale(6)]
        [Workspace(Default)]
        double Latitude { get; set; }

        #region Allors
        [Id("c51b6be6-5678-4664-b2c9-874cc46deb2e")]
        #endregion
        [Required]
        [Precision(9)]
        [Scale(6)]
        [Workspace(Default)]
        double Longitude { get; set; }
    }
}
