// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level1 in the setup and the security of every type: each records that it ran.
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void Level1Prepare(Setup setup) => this.Level1Log.Record("Level1", "ObjectsBase.Prepare(Setup)", this);

        protected virtual void Level1Setup(Setup setup) => this.Level1Log.Record("Level1", "ObjectsBase.Setup(Setup)", this);

        protected virtual void Level1Prepare(Security security) => this.Level1Log.Record("Level1", "ObjectsBase.Prepare(Security)", this);

        protected virtual void Level1Secure(Security security) => this.Level1Log.Record("Level1", "ObjectsBase.Secure(Security)", this);

        private ILevel1Log Level1Log => this.Transaction.Database.Services.Get<ILevel1Log>();
    }
}
