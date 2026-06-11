// <copyright file="Effect.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System;
    using Allors.Workspace.Signals;

    internal sealed class EffectNode : ComputationNode, IEffect
    {
        private readonly Action callback;
        private bool hasRun;

        internal EffectNode(Action callback, EffectScheduler scheduler)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            TrackingContext.ActiveScope?.AddEffect(this);
            scheduler.Enqueue(this);
        }

        internal EffectScheduler Scheduler { get; }

        internal EffectNode ScheduledNext { get; set; }

        internal EffectNode ScopeNext { get; set; }

        internal EffectNode ScopePrevious { get; set; }

        internal EffectScopeNode Scope { get; set; }

        internal override bool HasValue => this.hasRun;

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.Flags |= ReactiveFlags.Disposed;
            this.Scope?.RemoveEffect(this);
            this.DetachDependencies();
        }

        internal override bool Update()
        {
            if ((this.Flags & ReactiveFlags.Running) != 0)
            {
                return false;
            }

            this.Flags |= ReactiveFlags.Running;
            try
            {
                using (this.BeginTracking())
                {
                    this.callback();
                }

                this.hasRun = true;
                this.ClearDirtyFlags();
                return false;
            }
            finally
            {
                this.Flags &= ~ReactiveFlags.Running;
            }
        }

        internal void RunIfNeeded()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (!this.hasRun || Propagation.CheckDirty(this))
            {
                this.Update();
                return;
            }

            this.ClearDirtyFlags();
        }
    }
}
