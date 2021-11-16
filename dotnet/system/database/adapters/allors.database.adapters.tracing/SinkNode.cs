// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Collections.Generic;
    using Database.Tracing;

    public class SinkNode
    {
        public SinkNode(IEvent @event)
        {
            this.Event = @event;
            this.Nodes = new List<SinkNode>();
        }

        public IList<SinkNode> Nodes { get; }

        public IEvent Event { get; }

        public SinkNode OnBefore(IEvent @event)
        {
            var child = new SinkNode(@event);
            this.Nodes.Add(child);
            return child;
        }

        public override string ToString() => this.Event.ToString();
    }
}
