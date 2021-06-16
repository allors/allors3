// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Collections.Generic;
    using Meta;

    public interface IStrategy
    {
        IObject Object { get; }

        IClass Class { get; }

        long Id { get; }

        long Version { get; }

        ISession Session { get; }

        bool CanRead(IRoleType roleType);

        bool CanWrite(IRoleType roleType);

        bool CanExecute(IMethodType methodType);

        bool Exist(IRoleType roleType);

        object Get(IRoleType roleType);

        object GetUnit(IRoleType roleType);

        T GetComposite<T>(IRoleType roleType) where T : IObject;

        IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject;

        void Set(IRoleType roleType, object value);

        void SetUnit(IRoleType roleType, object value);

        void SetComposite<T>(IRoleType roleType, T value) where T : IObject;

        void SetComposites<T>(IRoleType roleType, in IEnumerable<T> role) where T : IObject;

        void Add<T>(IRoleType roleType, T value) where T : IObject;

        void Remove<T>(IRoleType roleType, T value) where T : IObject;

        void Remove(IRoleType roleType);

        T GetComposite<T>(IAssociationType associationType) where T : IObject;

        IEnumerable<T> GetComposites<T>(IAssociationType associationType) where T : IObject;
    }
}
