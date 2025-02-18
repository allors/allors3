// <copyright file="IWorkspace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Collections.Generic;

    public interface IDerivationError
    {
        string Message { get; }

        IEnumerable<Role> Roles { get; }
    }
}
