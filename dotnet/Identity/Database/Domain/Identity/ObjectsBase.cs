// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void IdentityPrepare(Setup setup)
        {
        }

        protected virtual void IdentitySetup(Setup setup)
        {
        }

        protected virtual void IdentityPrepare(Security security)
        {
        }

        protected virtual void IdentitySecure(Security security)
        {
        }
    }
}
