// <copyright file="SessionServices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;

    public partial class SessionServices : ISessionServices
    {
        public void Dispose()
        {
        }

        public void OnInit(ISession internalSession)
        {
        }

        public T Get<T>() =>
            typeof(T) switch
            {
                // Core
                //{ } type when type == typeof(M) => (T)this.M,
                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };
    }
}
