// <copyright file="ObjectsBase.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database;

    public abstract partial class ObjectsBase<T> where T : IObject
    {
        public void Prepare(Setup setup)
        {
            this.CorePrepare(setup);
            this.IdentityPrepare(setup);
            this.BasePrepare(setup);
            this.CustomPrepare(setup);
        }

        public void Setup(Setup setup)
        {
            this.CoreSetup(setup);
            this.IdentitySetup(setup);
            this.BaseSetup(setup);
            this.CustomSetup(setup);

            this.Transaction.Derive();
        }

        public void Prepare(Security security)
        {
            this.CorePrepare(security);
            this.IdentityPrepare(security);
            this.BasePrepare(security);
            this.CustomPrepare(security);
        }

        public void Secure(Security security)
        {
            this.CoreSecure(security);
            this.IdentitySecure(security);
            this.BaseSecure(security);
            this.CustomSecure(security);
        }
    }
}
