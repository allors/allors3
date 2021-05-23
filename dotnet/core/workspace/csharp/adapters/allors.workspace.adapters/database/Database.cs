// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Concurrent;
    using Meta;

    public abstract class DatabaseConnection : IDatabaseConnection
    {
        private ConcurrentDictionary<IObjectType, object> emptyArrayByObjectType;

        protected DatabaseConnection(Configuration configuration) => this.Configuration = configuration;

        IConfiguration IDatabaseConnection.Configuration => this.Configuration;
        public Configuration Configuration { get; }

        public abstract IWorkspace CreateWorkspace();

        public abstract DatabaseRecord GetRecord(long identity);

        public abstract long GetPermission(IClass @class, IOperandType operandType, Operations operation);

        public object EmptyArray(IObjectType objectType)
        {
            this.emptyArrayByObjectType ??= new ConcurrentDictionary<IObjectType, object>();

            if (this.emptyArrayByObjectType.TryGetValue(objectType, out var emptyArray))
            {
                return emptyArray;
            }

            var type = this.Configuration.ObjectFactory.GetType(objectType);
            emptyArray = Array.CreateInstance(type, 0);

            this.emptyArrayByObjectType.TryAdd(objectType, emptyArray);

            return emptyArray;
        }
    }
}
