// <copyright file="EffectScheduler.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    /// <summary>
    /// Batches and flushes scheduled effects. Not thread-safe — designed for
    /// single-threaded or async-only usage (via <see cref="TrackingContext"/>).
    /// All signal reads/writes and effect execution must occur on the same
    /// synchronization context.
    /// Instance-based to isolate effect queues per factory/session.
    /// </summary>
    internal sealed class EffectScheduler
    {
        private EffectNode head;
        private EffectNode tail;
        private int depth;

        internal void Enqueue(EffectNode node)
        {
            if (node.IsDisposed || (node.Flags & ReactiveFlags.Scheduled) != 0)
            {
                return;
            }

            node.Flags |= ReactiveFlags.Scheduled;

            if (this.tail == null)
            {
                this.head = node;
                this.tail = node;
            }
            else
            {
                this.tail.ScheduledNext = node;
                this.tail = node;
            }

            if (this.depth == 0)
            {
                this.Flush();
            }
        }

        // Hold/Release bracket a propagation walk: effects enqueued while held are
        // only flushed on the final Release. Flushing mid-walk would run an effect
        // before all of its dependencies are marked, letting it observe a torn
        // (partly stale) state and run a second, redundant time.
        internal void Hold() => this.depth++;

        internal void Release()
        {
            this.depth--;
            if (this.depth == 0)
            {
                this.Flush();
            }
        }

        private void Flush()
        {
            this.depth++;
            try
            {
                while (this.head != null)
                {
                    var node = this.head;
                    this.head = node.ScheduledNext;
                    if (this.head == null)
                    {
                        this.tail = null;
                    }

                    node.ScheduledNext = null;
                    node.Flags &= ~ReactiveFlags.Scheduled;
                    node.RunIfNeeded();
                }
            }
            finally
            {
                this.depth--;
            }
        }
    }
}
