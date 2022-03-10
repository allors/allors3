// <copyright file="DelegatedAccessControlledObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("842FA7B5-2668-43E9-BFEF-21B6F5B20E8B")]
    #endregion
    public partial interface DelegatedAccessObject : Object
    {
        #region Allors
        [Id("4277EB04-A800-4EA9-B19F-A2268D903D5F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Object DelegatedAccess { get; set; }
    }
}
