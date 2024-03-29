// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void LegacyPrepare(Setup setup)
        {
        }

        protected virtual void LegacySetup(Setup setup)
        {
        }

        protected virtual void LegacyPrepare(Security security)
        {
        }

        protected virtual void LegacySecure(Security config)
        {
        }
    }
}
