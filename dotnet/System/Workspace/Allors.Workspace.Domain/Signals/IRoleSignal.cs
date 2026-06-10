// <copyright file="IRoleSignal.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals
{
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Reactive signal for a scalar role property. Provides reactive read via
    /// <see cref="ISignal{T}.Value"/> and imperative write via <see cref="Set"/>.
    /// </summary>
    public interface IRoleSignal<T> : ISignal<T>
    {
        ISignal<bool> CanRead { get; }

        ISignal<bool> CanWrite { get; }

        ISignal<bool> Exists { get; }

        /// <summary>True if the current value differs from the last committed state.</summary>
        ISignal<bool> HasChanged { get; }

        /// <summary>
        /// Mutates the role value on the underlying strategy.
        /// For scalar roles, passing <c>null</c> clears the value.
        /// For composite roles, passing <c>null</c> removes the association.
        /// </summary>
        void Set(T value);

        /// <summary>Reverts the role to its last committed (database) value.</summary>
        void Restore();
    }

    /// <summary>
    /// Reactive signal for a collection (composites) role property.
    /// </summary>
    public interface ICompositesRoleSignal<T> : ISignal<IReadOnlyList<T>> where T : class, IObject
    {
        ISignal<bool> CanRead { get; }

        ISignal<bool> CanWrite { get; }

        ISignal<bool> Exists { get; }

        /// <summary>True if the current value differs from the last committed state.</summary>
        ISignal<bool> HasChanged { get; }

        void Set(IEnumerable<T> value);

        void Add(T value);

        void Remove(T value);

        /// <summary>Reverts the role to its last committed (database) value.</summary>
        void Restore();
    }

    /// <summary>
    /// Read-only reactive signal for a rule-computed (derived) property.
    /// </summary>
    public interface IDerivedRoleSignal<out T> : ISignal<T>
    {
        ISignal<bool> CanRead { get; }

        ISignal<bool> Exists { get; }
    }

    /// <summary>
    /// Read-only reactive signal for a reverse-navigation association (single composite).
    /// </summary>
    public interface IAssociationSignal<out T> : ISignal<T>
    {
    }

    /// <summary>
    /// Read-only reactive signal for a reverse-navigation association (collection).
    /// </summary>
    public interface ICompositesAssociationSignal<T> : ISignal<IReadOnlyList<T>> where T : class, IObject
    {
    }

    /// <summary>
    /// Represents a domain method with a reactive <see cref="CanExecute"/> signal
    /// indicating whether invocation is permitted.
    /// </summary>
    public interface IMethodSignal
    {
        IObject Object { get; }

        IMethodType MethodType { get; }

        ISignal<bool> CanExecute { get; }
    }
}
