// <copyright file="IEvent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Tracing
{
    using System;

    public interface IEvent
    {
        public ITransaction Transaction { get; }

        public DateTime Started { get; }

        public DateTime Stopped { get; }

        public TimeSpan Duration { get; }

        void Start();

        void Stop();
    }
}
