// <copyright file="DefaultSignalFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default
{
    using System;
    using System.Collections.Generic;
    using Allors.Workspace.Signals;
    using Allors.Workspace.Signals.Default.Core;

    public sealed class DefaultSignalFactory : ISignalFactory
    {
        internal EffectScheduler Scheduler { get; } = new EffectScheduler();

        public IStateSignal<T> State<T>(T initialValue, IEqualityComparer<T> comparer = null) =>
            new StateSignal<T>(initialValue, comparer ?? EqualityComparer<T>.Default);

        public IComputedSignal<T> Computed<T>(Func<T> computation, IEqualityComparer<T> comparer = null) =>
            new ComputedSignalNode<T>(computation, comparer);

        public IEffect Effect(Action callback) => new EffectNode(callback, this.Scheduler);

        public IEffectScope EffectScope() => new EffectScopeNode();
    }
}
