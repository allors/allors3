// <copyright file="StrategySignals.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Signals;

    internal sealed class RoleSignal<T> : IRoleSignal<T>
    {
        private readonly Strategy strategy;
        private readonly IRoleType roleType;
        private readonly ISignal<T> value;
        private readonly Action<T> setter;

        internal RoleSignal(Strategy strategy, IRoleType roleType, Func<T> getter, Action<T> setter)
        {
            this.strategy = strategy;
            this.roleType = roleType;
            this.setter = setter;
            this.value = strategy.Session.SignalFactory.Computed(() =>
            {
                _ = strategy.Session.GraphRevision.Value;
                return getter();
            });

            this.CanRead = strategy.GetCanReadSignal(roleType);
            this.CanWrite = strategy.GetCanWriteSignal(roleType);
            this.Exists = strategy.GetExistRoleSignal(roleType);
            this.HasChanged = strategy.GetHasChangedSignal(roleType);
        }

        public ISignal<bool> CanRead { get; }

        public ISignal<bool> CanWrite { get; }

        public ISignal<bool> Exists { get; }

        public ISignal<bool> HasChanged { get; }

        public T Value => this.value.Value;

        public void Set(T value) => this.setter(value);

        public void Restore() => this.strategy.RestoreRole(this.roleType);
    }

    internal sealed class CompositesRoleSignal<T> : ICompositesRoleSignal<T> where T : class, IObject
    {
        private readonly Strategy strategy;
        private readonly IRoleType roleType;
        private readonly ISignal<IReadOnlyList<T>> value;

        internal CompositesRoleSignal(Strategy strategy, IRoleType roleType)
        {
            this.strategy = strategy;
            this.roleType = roleType;
            this.value = strategy.Session.SignalFactory.Computed<IReadOnlyList<T>>(() =>
            {
                _ = strategy.Session.GraphRevision.Value;
                return strategy.GetCompositesRole<T>(roleType).ToArray();
            });

            this.CanRead = strategy.GetCanReadSignal(roleType);
            this.CanWrite = strategy.GetCanWriteSignal(roleType);
            this.Exists = strategy.GetExistRoleSignal(roleType);
            this.HasChanged = strategy.GetHasChangedSignal(roleType);
        }

        public ISignal<bool> CanRead { get; }

        public ISignal<bool> CanWrite { get; }

        public ISignal<bool> Exists { get; }

        public ISignal<bool> HasChanged { get; }

        public IReadOnlyList<T> Value => this.value.Value;

        public void Set(IEnumerable<T> value) => this.strategy.SetCompositesRole(this.roleType, value);

        public void Add(T value) => this.strategy.AddCompositesRole(this.roleType, value);

        public void Remove(T value) => this.strategy.RemoveCompositesRole(this.roleType, value);

        public void Restore() => this.strategy.RestoreRole(this.roleType);
    }

    internal sealed class DerivedRoleSignal<T> : IDerivedRoleSignal<T>
    {
        private static readonly Type ReadOnlyListType = typeof(IReadOnlyList<>);
        private readonly ISignal<T> value;

        internal DerivedRoleSignal(Strategy strategy, IRoleType roleType)
        {
            this.value = strategy.Session.SignalFactory.Computed(() =>
            {
                _ = strategy.Session.GraphRevision.Value;
                return ConvertValue(strategy.GetRole(roleType));
            });

            this.CanRead = strategy.GetCanReadSignal(roleType);
            this.Exists = strategy.GetExistRoleSignal(roleType);
        }

        public ISignal<bool> CanRead { get; }

        public ISignal<bool> Exists { get; }

        public T Value => this.value.Value;

        private static T ConvertValue(object value)
        {
            if (value == null)
            {
                return default;
            }

            if (value is T typed)
            {
                return typed;
            }

            var resultType = typeof(T);
            if (value is IEnumerable enumerable &&
                resultType.IsGenericType &&
                resultType.GetGenericTypeDefinition() == ReadOnlyListType)
            {
                var elementType = resultType.GetGenericArguments()[0];
                var items = enumerable
                    .Cast<object>()
                    .Select(item => ConvertItem(item, elementType))
                    .ToArray();
                var array = Array.CreateInstance(elementType, items.Length);
                for (var i = 0; i < items.Length; i++)
                {
                    array.SetValue(items[i], i);
                }

                return (T)(object)array;
            }

            return (T)value;
        }

        private static object ConvertItem(object item, Type targetType)
        {
            if (item == null || targetType.IsInstanceOfType(item))
            {
                return item;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlyingType.IsInstanceOfType(item))
            {
                return item;
            }

            return Convert.ChangeType(item, underlyingType);
        }
    }

    internal sealed class AssociationSignal<T> : IAssociationSignal<T> where T : class, IObject
    {
        private readonly ISignal<T> value;

        internal AssociationSignal(Strategy strategy, IAssociationType associationType)
        {
            this.value = strategy.Session.SignalFactory.Computed(() =>
            {
                _ = strategy.Session.GraphRevision.Value;
                return strategy.GetCompositeAssociation<T>(associationType);
            });
        }

        public T Value => this.value.Value;
    }

    internal sealed class CompositesAssociationSignal<T> : ICompositesAssociationSignal<T> where T : class, IObject
    {
        private readonly ISignal<IReadOnlyList<T>> value;

        internal CompositesAssociationSignal(Strategy strategy, IAssociationType associationType)
        {
            this.value = strategy.Session.SignalFactory.Computed<IReadOnlyList<T>>(() =>
            {
                _ = strategy.Session.GraphRevision.Value;
                return strategy.GetCompositesAssociation<T>(associationType).ToArray();
            });
        }

        public IReadOnlyList<T> Value => this.value.Value;
    }

    internal sealed class MethodSignal : IMethodSignal
    {
        internal MethodSignal(Strategy strategy, IMethodType methodType)
        {
            this.Object = strategy.Object;
            this.MethodType = methodType;
            this.CanExecute = strategy.GetCanExecuteSignal(methodType);
        }

        public IObject Object { get; }

        public IMethodType MethodType { get; }

        public ISignal<bool> CanExecute { get; }
    }
}
