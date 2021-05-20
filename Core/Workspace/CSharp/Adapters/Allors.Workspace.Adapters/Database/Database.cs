// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Meta;

    public abstract class Database
    {
        protected Database(IMetaPopulation metaPopulation, WorkspaceIdGenerator workspaceIdGenerator = null)
        {
            this.MetaPopulation = metaPopulation;
            this.WorkspaceIdGenerator = workspaceIdGenerator ?? new WorkspaceIdGenerator();
        }

        public IMetaPopulation MetaPopulation { get; }

        public WorkspaceIdGenerator WorkspaceIdGenerator { get; }

        public abstract DatabaseRecord GetRecord(long identity);

        public abstract long GetPermission(IClass @class, IOperandType operandType, Operations operation);
    }
}
