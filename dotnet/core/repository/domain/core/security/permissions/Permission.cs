// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("7fded183-3337-4196-afb0-3266377944bc")]
    #endregion
    public partial interface Permission : Deletable
    {
        #region Allors
        [Id("29b80857-e51b-4dec-b859-887ed74b1626")]
        [Indexed]
        #endregion
        [Required]
        public Guid ClassPointer { get; set; }
    }
}
