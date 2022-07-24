// <copyright file="ObjectState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("f991813f-3146-4431-96d0-554aa2186887")]
    #endregion
    public partial interface ObjectState : UniquelyIdentifiable
    {
        #region Allors
        [Id("913C994F-15B0-40D2-AC4F-81E362B9142C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        Revocation ObjectRevocation { get; set; }

        #region Allors
        [Id("b86f9e42-fe10-4302-ab7c-6c6c7d357c39")]
        #endregion
        [Workspace(Default)]
        [Indexed]
        [Size(256)]
        string Name { get; set; }
    }
}
