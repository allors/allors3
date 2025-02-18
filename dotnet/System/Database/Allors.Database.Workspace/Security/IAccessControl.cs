// <copyright file="AccessControlListFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Security
{
    public interface IAccessControl
    {
        IAccessControlList this[IObject @object]
        {
            get;
        }
    }
}
