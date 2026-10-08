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

        /// <summary>
        /// The fingerprint of the workspace's meta: <see cref="MetaFingerprint"/> over the tags
        /// of the composites, relation types and method types that are in the workspace, which
        /// a client computes from the workspace meta generated for that workspace.
        /// </summary>
        string GetWorkspaceFingerprint(string workspaceName);
    }
}
