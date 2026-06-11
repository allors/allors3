// <copyright file="Object.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Collections.Generic;
    using Meta;
    using Signals;

    public interface IStrategy
    {
        ISession Session { get; }

        IObject Object { get; }

        IClass Class { get; }

        long Id { get; }

        ISignal<long> Version { get; }

        ISignal<bool> IsNew { get; }

        ISignal<bool> HasChanges { get; }

        void Reset();

        IReadOnlyList<IDiff> Diff();

        ISignal<bool> CanRead(IRoleType roleType);

        ISignal<bool> CanWrite(IRoleType roleType);

        ISignal<bool> CanExecute(IMethodType methodType);

        ISignal<bool> ExistRole(IRoleType roleType);

        ISignal<bool> HasChanged(IRoleType roleType);

        void RestoreRole(IRoleType roleType);

        IRoleSignal<object> ScalarRole(IRoleType roleType);

        IRoleSignal<T> ScalarRole<T>(IRoleType roleType);

        IRoleSignal<T> CompositeRole<T>(IRoleType roleType) where T : class, IObject;

        ICompositesRoleSignal<T> CompositesRole<T>(IRoleType roleType) where T : class, IObject;

        IDerivedRoleSignal<T> DerivedRole<T>(IRoleType roleType);

        IAssociationSignal<T> CompositeAssociation<T>(IAssociationType associationType) where T : class, IObject;

        ICompositesAssociationSignal<T> CompositesAssociation<T>(IAssociationType associationType) where T : class, IObject;

        IMethodSignal Method(IMethodType methodType);
    }
}
