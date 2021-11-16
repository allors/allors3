// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System;
    using System.Collections.Generic;
    using Database.Tracing;

    public class SinkTree
    {
        private Stack<SinkNode> stack;

        public SinkTree(ITransaction transaction, long index)
        {
            this.Transaction = transaction;
            this.Index = index;

            this.stack = new Stack<SinkNode>();
            this.Nodes = new List<SinkNode>();
        }

        public ITransaction Transaction { get; }

        public IList<SinkNode> Nodes { get; private set; }

        public long Index { get; set; }

        public void OnBefore(IEvent @event)
        {
            SinkNode sinkNode;

            if (this.stack.Count > 0)
            {
                var top = this.stack.Peek();
                sinkNode = top.OnBefore(@event);
            }
            else
            {
                sinkNode = new SinkNode(@event);
                this.Nodes.Add(sinkNode);
            }

            this.stack.Push(sinkNode);
        }

        public void OnAfter(IEvent @event)
        {
            var top = this.stack.Pop();
            if (top.Event != @event)
            {
                throw new ArgumentException("Events are out of sync");
            }
        }

        public void Clear()
        {
            this.stack = new Stack<SinkNode>();
            this.Nodes = new List<SinkNode>();
        }

        public override string ToString() => this.Transaction.ToString();
    }
}
