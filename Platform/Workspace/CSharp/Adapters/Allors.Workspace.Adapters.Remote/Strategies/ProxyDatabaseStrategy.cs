// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Meta;

    public class ProxyDatabaseStrategy : IStrategy
    {
        public ProxyDatabaseStrategy(IDatabaseStrategy strategy) => this.Strategy = strategy;

        public IDatabaseStrategy Strategy
        {
            get;
            set;
        }

        public IObject Object => this.Strategy.Object;

        public long WorkspaceId
        {
            get => this.Strategy.WorkspaceId;
            set => this.Strategy.WorkspaceId = value;
        }

        public long? DatabaseId => this.Strategy.DatabaseId;

        public long? Version => this.Strategy.Version;

        public IClass Class => this.Strategy.Class;

        public ISession Session => this.Strategy.Session;

        public bool HasDatabaseChanges => this.Strategy.HasDatabaseChanges;

        public bool CanRead(IRoleType roleType) => this.Strategy.CanRead(roleType);

        public bool CanWrite(IRoleType roleType) => this.Strategy.CanWrite(roleType);

        public bool CanExecute(IMethodType methodType) => this.Strategy.CanExecute(methodType);

        public bool Exist(IRoleType roleType) => this.Strategy.Exist(roleType);

        public object Get(IRoleType roleType) => this.Strategy.Get(roleType);

        public void Set(IRoleType roleType, object value) => this.Strategy.Set(roleType, value);

        public void Add(IRoleType roleType, IObject value) => this.Strategy.Add(roleType, value);

        public void Remove(IRoleType roleType, IObject value) => this.Strategy.Remove(roleType, value);

        public object GetAssociation(IAssociationType associationType) => this.Strategy.GetAssociation(associationType);

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType) => this.Strategy.GetAssociations(associationType);
    }
}
