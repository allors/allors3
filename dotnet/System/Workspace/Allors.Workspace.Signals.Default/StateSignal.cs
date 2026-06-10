// <copyright file="StateSignal.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default
{
    using System.Collections.Generic;
    using Allors.Workspace.Signals;
    using Allors.Workspace.Signals.Default.Core;

    /// <summary>
    /// Writable signal that holds mutable state. Not thread-safe — all access
    /// must occur on the same synchronization context as the reactive graph.
    /// </summary>
    internal sealed class StateSignal<T> : ReactiveNode, IStateSignal<T>
    {
        private readonly IEqualityComparer<T> comparer;
        private T value;

        internal StateSignal(T value, IEqualityComparer<T> comparer)
        {
            this.value = value;
            this.comparer = comparer;

            // Initial version is 1 (not 0) so newly created links with Version = 0
            // are always considered stale on first access.
            this.Version = 1;
        }

        public T Value
        {
            get
            {
                this.TrackRead();
                return this.value;
            }

            set
            {
                if (this.comparer.Equals(this.value, value))
                {
                    return;
                }

                this.value = value;
                this.Version++;
                Propagation.Propagate(this);
            }
        }
    }
}
