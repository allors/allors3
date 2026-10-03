// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void EntraPrepare(Setup setup)
        {
        }

        protected virtual void EntraSetup(Setup setup)
        {
        }

        protected virtual void EntraPrepare(Security security)
        {
        }

        protected virtual void EntraSecure(Security security)
        {
        }
    }
}
