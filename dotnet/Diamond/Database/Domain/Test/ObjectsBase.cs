// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of the concrete domain in the setup and the security of every type: each records that it ran.
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void TestPrepare(Setup setup) => this.TestLog.Record("Test", "ObjectsBase.Prepare(Setup)", this);

        protected virtual void TestSetup(Setup setup) => this.TestLog.Record("Test", "ObjectsBase.Setup(Setup)", this);

        protected virtual void TestPrepare(Security security) => this.TestLog.Record("Test", "ObjectsBase.Prepare(Security)", this);

        protected virtual void TestSecure(Security security) => this.TestLog.Record("Test", "ObjectsBase.Secure(Security)", this);

        private ILevel1Log TestLog => this.Transaction.Database.Services.Get<ILevel1Log>();
    }
}
