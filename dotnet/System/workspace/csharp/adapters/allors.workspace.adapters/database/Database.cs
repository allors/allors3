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

        private readonly IdGenerator idGenerator;

        protected DatabaseConnection(Configuration configuration, IdGenerator idGenerator)
        {
            this.Configuration = configuration;
            this.idGenerator = idGenerator;
        }

        IConfiguration IDatabaseConnection.Configuration => this.Configuration;
        public Configuration Configuration { get; }

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

        public abstract IWorkspace CreateWorkspace();

        public abstract DatabaseRecord GetRecord(long id);

        public abstract long GetPermission(IClass @class, IOperandType operandType, Operations operation);

        public long NextId() => this.idGenerator.Next();
    }
}
