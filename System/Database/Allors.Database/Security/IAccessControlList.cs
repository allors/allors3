// <copyright file="AccessControlList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Security
{
    using System.Collections.Generic;
    using Allors.Database.Meta;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public interface IAccessControlList
    {
        IEnumerable<IAccessControl> AccessControls { get; }

        ISet<long> DeniedPermissionIds { get; }

        bool CanRead(IRoleType roleType);

        bool CanWrite(IRoleType roleType);

        bool CanExecute(IMethodType methodType);
    }
}
