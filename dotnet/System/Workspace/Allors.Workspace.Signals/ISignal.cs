// <copyright file="ISignal.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A readable reactive value. Reading <see cref="Value"/> inside a computed or
    /// effect automatically registers a dependency on this signal.
    /// </summary>
    public interface ISignal<out T>
    {
        T Value { get; }
    }

    /// <summary>
    /// A writable signal that holds mutable state. Setting <see cref="Value"/>
    /// propagates change notifications to all dependent computed signals and effects.
    /// </summary>
    public interface IStateSignal<T> : ISignal<T>
    {
        new T Value { get; set; }
    }

    /// <summary>
    /// A lazily evaluated, memoized signal. Recomputes only when one or more of
    /// its tracked dependencies have changed since the last evaluation.
    /// Dispose to detach from the reactive graph and allow garbage collection.
    /// </summary>
    public interface IComputedSignal<out T> : ISignal<T>, IDisposable
    {
    }

    /// <summary>
    /// A side-effect that automatically re-runs when any signal read during
    /// its last execution changes. Dispose to stop the effect.
    /// </summary>
    public interface IEffect : IDisposable
    {
    }

    /// <summary>
    /// Groups multiple effects for collective disposal. Disposing the scope
    /// disposes all effects created within it.
    /// </summary>
    public interface IEffectScope : IDisposable
    {
    }

    /// <summary>
    /// Factory abstraction for creating reactive primitives. Implementations
    /// provide the underlying reactive engine (e.g. push-pull hybrid).
    /// </summary>
    public interface ISignalFactory
    {
        IStateSignal<T> State<T>(T initialValue, IEqualityComparer<T> comparer = null);

        /// <summary>
        /// Creates a computed signal. The optional <paramref name="comparer"/> decides
        /// whether a recomputed value counts as changed; pass one when the computation
        /// rebuilds equal values (e.g. collections) so unchanged results don't propagate.
        /// </summary>
        IComputedSignal<T> Computed<T>(Func<T> computation, IEqualityComparer<T> comparer = null);

        IEffect Effect(Action callback);

        IEffectScope EffectScope();
    }
}
