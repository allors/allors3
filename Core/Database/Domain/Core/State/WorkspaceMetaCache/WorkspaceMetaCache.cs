// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System.Collections.Generic;
    using System.Linq;

    public class WorkspaceMetaCache : IWorkspaceMetaCache
    {
        private readonly Dictionary<string, IWorkspaceMetaCacheEntry> workspaceMetaCacheEntryByWorkspaceName;

        public WorkspaceMetaCache(IDatabaseState databaseState)
        {
            var metaPopulation = databaseState.MetaPopulation;

            this.workspaceMetaCacheEntryByWorkspaceName = metaPopulation.WorkspaceNames.ToDictionary(v => v,
                v => (IWorkspaceMetaCacheEntry)new WorkspaceMetaCacheEntry(metaPopulation, v));
        }

        public IWorkspaceMetaCacheEntry Get(string workspaceName)
        {
            this.workspaceMetaCacheEntryByWorkspaceName.TryGetValue(workspaceName, out var workspaceMetaCacheEntry);
            return workspaceMetaCacheEntry;
        }
    }
}
