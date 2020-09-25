// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("7fded183-3337-4196-afb0-3266377944bc")]
    #endregion
    public partial interface Permission : Deletable
    {
        #region Allors
        [Id("29b80857-e51b-4dec-b859-887ed74b1626")]
        [AssociationId("8ffed1eb-b64e-4341-bbb6-348ed7f06e83")]
        [RoleId("cadaca05-55ba-4a13-8758-786ff29c8e46")]
        [Indexed]
        #endregion
        [Required]
        public Guid ConcreteClassPointer { get; set; }
    }
}
