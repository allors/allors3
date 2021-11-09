// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Collections.Concurrent;

    public class Sink : ISink
    {
        public Sink() => this.TreeByTransaction = new ConcurrentDictionary<ITransaction, SinkTree>();

        public ConcurrentDictionary<ITransaction, SinkTree> TreeByTransaction { get; }

        public void OnBefore(Event @event)
        {
            var transactionSink = this.GetTransactionSink(@event);
            transactionSink.OnBefore(@event);
        }

        public void OnAfter(Event @event)
        {
            var transactionSink = this.GetTransactionSink(@event);
            transactionSink.OnAfter(@event);
        }

        private SinkTree GetTransactionSink(Event @event) => this.TreeByTransaction.GetOrAdd(@event.Transaction, (v) => new SinkTree(v));
    }
}
