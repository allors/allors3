// <copyright file="ITrace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Tracing
{
    using System;
    using System.Text;
    using Allors.Database.Tracing;

    public abstract class Event : IEvent
    {
        protected Event(ITransaction transaction) => this.Transaction = transaction;

        public ITransaction Transaction { get; }

        public DateTime Started { get; private set; }

        public DateTime Stopped { get; private set; }

        public TimeSpan Duration => this.Stopped - this.Started;

        public void Start() => this.Started = DateTime.Now;

        public void Stop() => this.Stopped = DateTime.Now;

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(this.GetType().Name);

            this.ToString(builder);

            if (this.Duration > new TimeSpan(0, 0, 0, 0, 1))
            {
                builder.Append(" (")
                    .Append(this.Duration.ToString("s\\.fff"))
                    .Append("s)");
            }

            return builder.ToString();
        }

        protected abstract void ToString(StringBuilder builder);
    }
}
