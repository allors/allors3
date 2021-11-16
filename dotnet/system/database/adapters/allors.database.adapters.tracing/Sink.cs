// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using Database.Tracing;

    public class Sink : ISink
    {
        private int counter;

        public Sink()
        {
            this.counter = 0;
            this.TreeByTransaction = new ConcurrentDictionary<ITransaction, SinkTree>();
        }

        public ConcurrentDictionary<ITransaction, SinkTree> TreeByTransaction { get; }

        public Func<IEvent, bool> Breaker { get; set; }

        public SinkTree[] Trees => this.TreeByTransaction
            .Values
            .OrderBy(v => v.Index)
            .ToArray();

        public void OnBefore(IEvent @event)
        {
            var transactionSink = this.GetTransactionSink(@event);

            if (this.Breaker != null && this.Breaker(@event))
            {
                Debugger.Break();
            }

            transactionSink.OnBefore(@event);
        }

        public void OnAfter(IEvent @event)
        {
            var transactionSink = this.GetTransactionSink(@event);
            transactionSink.OnAfter(@event);
            @event.Stop();
        }

        private SinkTree GetTransactionSink(IEvent @event) => this.TreeByTransaction.GetOrAdd(@event.Transaction, (v) => new SinkTree(v, ++this.counter));
    }
}
