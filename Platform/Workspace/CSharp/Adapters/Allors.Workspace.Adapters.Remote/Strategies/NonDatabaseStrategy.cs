// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Meta;

    public abstract class NonDatabaseStrategy : Strategy, IStrategy
    {
        private IObject @object;

        protected NonDatabaseStrategy(Session session, IClass @class, long workspaceId) : base(session, workspaceId) => this.Class = @class;

        public IObject Object
        {
            get
            {
                this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);
                return this.@object;
            }
        }

        public IClass Class { get; set; }

        public long? DatabaseId => null;

        public long? Version => null;

        ISession IStrategy.Session => this.Session;

        public bool CanRead(IRoleType roleType) => true;

        public bool CanWrite(IRoleType roleType) => true;

        public bool CanExecute(IMethodType methodType) => true;
    }
}
