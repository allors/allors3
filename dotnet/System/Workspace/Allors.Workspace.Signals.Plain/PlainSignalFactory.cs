// <copyright file="PlainSignalFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals
{
    using System;
    using System.Collections.Generic;

    public sealed class PlainSignalFactory : ISignalFactory
    {
        public IStateSignal<T> State<T>(T initialValue, IEqualityComparer<T> comparer = null) =>
            new PlainStateSignal<T>(initialValue, comparer ?? EqualityComparer<T>.Default);

        public IComputedSignal<T> Computed<T>(Func<T> computation) =>
            new PlainComputedSignal<T>(computation ?? throw new ArgumentNullException(nameof(computation)));

        public IEffect Effect(Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            callback();
            return NoOpEffect.Instance;
        }

        public IEffectScope EffectScope() => NoOpEffectScope.Instance;

        private sealed class PlainStateSignal<T> : IStateSignal<T>
        {
            private readonly IEqualityComparer<T> comparer;
            private T value;

            internal PlainStateSignal(T value, IEqualityComparer<T> comparer)
            {
                this.value = value;
                this.comparer = comparer;
            }

            public T Value
            {
                get => this.value;
                set
                {
                    if (!this.comparer.Equals(this.value, value))
                    {
                        this.value = value;
                    }
                }
            }
        }

        private sealed class PlainComputedSignal<T> : IComputedSignal<T>
        {
            private readonly Func<T> computation;

            internal PlainComputedSignal(Func<T> computation) => this.computation = computation;

            public T Value => this.computation();

            public void Dispose()
            {
            }
        }

        private sealed class NoOpEffect : IEffect
        {
            internal static readonly NoOpEffect Instance = new NoOpEffect();

            public void Dispose()
            {
            }
        }

        private sealed class NoOpEffectScope : IEffectScope
        {
            internal static readonly NoOpEffectScope Instance = new NoOpEffectScope();

            public void Dispose()
            {
            }
        }
    }
}
