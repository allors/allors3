// <copyright file="ObjectsBase.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T> where T : IObject
    {
        public void Prepare(Setup setup)
        {
            this.CorePrepare(setup);
            this.BasePrepare(setup);
            this.AppsPrepare(setup);
        }

        public void Setup(Setup setup)
        {
            this.CoreSetup(setup);
            this.BaseSetup(setup);
            this.AppsSetup(setup);
        }

        public void Prepare(Security security)
        {
            this.CorePrepare(security);
            this.BasePrepare(security);
            this.AppsPrepare(security);
        }

        public void Secure(Security security)
        {
            this.CoreSecure(security);
            this.BaseSecure(security);
            this.AppsSecure(security);
        }
    }
}
