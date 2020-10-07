// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class WorkspaceMetaCacheEntry : IWorkspaceMetaCacheEntry
    {
        public WorkspaceMetaCacheEntry(MetaPopulation metaPopulation, string workspaceName)
        {
            this.Classes =  new HashSet<Class>(metaPopulation.Classes.Where(v => v.WorkspaceNames.Contains(workspaceName)));
        }

        public ISet<Class> Classes { get; }
    }
}
