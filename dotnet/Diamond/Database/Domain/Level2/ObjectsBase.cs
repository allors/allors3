// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level2 in the setup and the security of every type: each records that it ran.
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void Level2Prepare(Setup setup) => this.Level2Log.Record("Level2", "ObjectsBase.Prepare(Setup)", this);

        protected virtual void Level2Setup(Setup setup) => this.Level2Log.Record("Level2", "ObjectsBase.Setup(Setup)", this);

        protected virtual void Level2Prepare(Security security) => this.Level2Log.Record("Level2", "ObjectsBase.Prepare(Security)", this);

        protected virtual void Level2Secure(Security security) => this.Level2Log.Record("Level2", "ObjectsBase.Secure(Security)", this);

        private ILevel1Log Level2Log => this.Transaction.Database.Services.Get<ILevel1Log>();
    }
}
