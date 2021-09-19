// <copyright file="ObjectsBase.v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T> where T : IObject
    {
        public void Prepare(Setup setup)
        {
            this.CorePrepare(setup);
            this.CustomPrepare(setup);
        }

        public void Setup(Setup setup)
        {
            this.CoreSetup(setup);
            this.CustomSetup(setup);

            this.Transaction.Derive();
        }

        public void Prepare(Security security)
        {
            this.CorePrepare(security);
            this.CustomPrepare(security);
        }

        public void Secure(Security security)
        {
            this.CoreSecure(security);
            this.CustomSecure(security);
        }
    }
}
