// <copyright file="ITreeCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    // TODO: Remove
    public interface IWorkspaceMask
    {
        IDictionary<IClass, IRoleType> GetMasks(string workspaceName);
    }
}
