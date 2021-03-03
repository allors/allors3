// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Meta;

    public abstract class RemoteStrategy : IStrategy
    {
        public abstract IObject Object { get; }

        public abstract Identity Identity { get; }

        public abstract IClass Class { get; }

        ISession IStrategy.Session => this.Session;

        public abstract RemoteSession Session { get; }

        public abstract bool Exist(IRoleType roleType);

        public abstract object Get(IRoleType roleType);

        public abstract void Set(IRoleType roleType, object value);

        public abstract void Add(IRoleType roleType, IObject value);

        public abstract void Remove(IRoleType roleType, IObject value);

        public abstract IObject GetAssociation(IAssociationType associationType);

        public abstract IEnumerable<IObject> GetAssociations(IAssociationType associationType);

        public abstract bool CanRead(IRoleType roleType);

        public abstract bool CanWrite(IRoleType roleType);

        public abstract bool CanExecute(IMethodType methodType);
    }
}
