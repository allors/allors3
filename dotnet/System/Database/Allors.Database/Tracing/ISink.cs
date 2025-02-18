// <copyright file="ITrace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Tracing
{
    public interface ISink
    {
        void OnBefore(IEvent @event);

        void OnAfter(IEvent @event);
    }
}
