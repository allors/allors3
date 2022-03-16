// <copyright file="Cacheable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("42A5253D-3F00-41CE-8852-DF637C98B667")]
    #endregion
    public partial interface SecurityCacheable : Object
    {
        #region Allors
        [Id("E8F9F42D-7B84-4851-8FF9-3390C842DBA6")]
        #endregion
        [Derived]
        Guid SecurityCacheId { get; set; }
    }
}
