// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class ObjectsBase<T>
    {
        protected virtual void CustomPrepare(Setup setup)
        {
        }

        protected virtual void CustomSetup(Setup setup)
        {
        }

        protected virtual void CustomPrepare(Security security)
        {
        }

        protected virtual void CustomSecure(Security security)
        {
        }
    }
}
