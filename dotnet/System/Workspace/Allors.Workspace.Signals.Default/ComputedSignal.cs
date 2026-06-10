// <copyright file="ComputedSignal.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default
{
    using System;
    using System.Collections.Generic;
    using Allors.Workspace.Signals;
    using Allors.Workspace.Signals.Default.Core;

    internal sealed class ComputedSignalNode<T> : ComputedNode, IComputedSignal<T>
    {
        private readonly Func<T> computation;
        private readonly IEqualityComparer<T> comparer;
        private T value;
        private bool hasValue;

        internal ComputedSignalNode(Func<T> computation)
        {
            this.computation = computation ?? throw new ArgumentNullException(nameof(computation));
            this.comparer = EqualityComparer<T>.Default;
        }

        public T Value
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                this.EnsureValue();
                this.TrackRead();
                return this.value;
            }
        }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.Flags |= ReactiveFlags.Disposed;
            this.DetachDependencies();
            this.DetachSubscribers();
        }

        internal override bool HasValue => this.hasValue;

        internal override bool Update()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if ((this.Flags & ReactiveFlags.Running) != 0)
            {
                throw new InvalidOperationException("Circular signal dependency detected.");
            }

            this.Flags |= ReactiveFlags.Running;
            try
            {
                T nextValue;
                using (this.BeginTracking())
                {
                    nextValue = this.computation();
                }

                var changed = !this.hasValue || !this.comparer.Equals(this.value, nextValue);
                this.value = nextValue;
                this.hasValue = true;
                if (changed)
                {
                    this.Version++;
                }

                this.ClearDirtyFlags();
                return changed;
            }
            finally
            {
                this.Flags &= ~ReactiveFlags.Running;
            }
        }

        private void EnsureValue()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (!this.NeedsEvaluation)
            {
                return;
            }

            if (!this.hasValue || Propagation.CheckDirty(this))
            {
                this.Update();
            }
            else
            {
                this.ClearDirtyFlags();
            }
        }
    }
}
