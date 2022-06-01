// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System;
    using System.Collections.Concurrent;
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

        public Action<IEvent> PreOnBefore { get; set; }

        public Action<IEvent> PostOnBefore { get; set; }

        public Action<IEvent> PreOnAfter { get; set; }

        public Action<IEvent> PostOnAfter { get; set; }

        public SinkTree[] Trees => this.TreeByTransaction
            .Values
            .OrderBy(v => v.Index)
            .ToArray();

        public void OnBefore(IEvent @event)
        {
            @event.Start();

            this.PreOnBefore?.Invoke(@event);

            var sinkTree = this.GetTransactionSink(@event);
            sinkTree.OnBefore(@event);

            this.PostOnBefore?.Invoke(@event);
        }

        public void OnAfter(IEvent @event)
        {
            this.PreOnAfter?.Invoke(@event);

            var sinkTree = this.GetTransactionSink(@event);
            sinkTree.OnAfter(@event);
            @event.Stop();

            this.PostOnAfter?.Invoke(@event);
        }

        private SinkTree GetTransactionSink(IEvent @event) => this.TreeByTransaction.GetOrAdd(@event.Transaction, (v) => new SinkTree(v, ++this.counter));
    }
}
