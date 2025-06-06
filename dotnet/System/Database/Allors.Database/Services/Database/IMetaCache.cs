// <copyright file="IMetaCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Services
{
    using System;
    using System.Collections.Generic;
    using Meta;

    public interface IMetaCache
    {
        Type GetBuilderType(IClass @class);

        ISet<IClass> GetWorkspaceClasses(string workspaceName);

        IDictionary<IClass, ISet<IRoleType>> GetWorkspaceRoleTypesByClass(string workspaceName);
    }
}
