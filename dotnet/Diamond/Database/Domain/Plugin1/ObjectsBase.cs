// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Plugin1 in the setup and the security of every type: each records that it ran.
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void Plugin1Prepare(Setup setup) => this.Plugin1Log.Record("Plugin1", "ObjectsBase.Prepare(Setup)", this);

        protected virtual void Plugin1Setup(Setup setup) => this.Plugin1Log.Record("Plugin1", "ObjectsBase.Setup(Setup)", this);

        protected virtual void Plugin1Prepare(Security security) => this.Plugin1Log.Record("Plugin1", "ObjectsBase.Prepare(Security)", this);

        protected virtual void Plugin1Secure(Security security) => this.Plugin1Log.Record("Plugin1", "ObjectsBase.Secure(Security)", this);

        private IPlugin1Log Plugin1Log => this.Transaction.Database.Services.Get<IPlugin1Log>();
    }
}
