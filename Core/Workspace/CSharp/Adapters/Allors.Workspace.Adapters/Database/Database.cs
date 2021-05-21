// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Meta;

    public abstract class DatabaseConnection : IDatabaseConnection
    {
        protected DatabaseConnection(Configuration configuration) => this.Configuration = configuration;

        IConfiguration IDatabaseConnection.Configuration => this.Configuration;
        public Configuration Configuration { get; }

        public abstract IWorkspace CreateWorkspace();

        public IMetaPopulation MetaPopulation => this.Configuration.MetaPopulation;

        public WorkspaceIdGenerator WorkspaceIdGenerator => this.Configuration.workspaceIdGenerator;

        public abstract DatabaseRecord GetRecord(long identity);

        public abstract long GetPermission(IClass @class, IOperandType operandType, Operations operation);
    }
}
