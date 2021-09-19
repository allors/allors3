// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void AppsPrepare(Setup setup)
        {
        }

        protected virtual void AppsSetup(Setup setup)
        {
        }

        protected virtual void AppsPrepare(Security security)
        {
        }

        protected virtual void AppsSecure(Security security)
        {
        }
    }
}
