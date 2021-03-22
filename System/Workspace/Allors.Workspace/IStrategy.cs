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
        IClass Class { get; }

        long Id { get; }

        IObject Object { get; }

        ISession Session { get; }

        bool Exist(IRoleType roleType);

        object GetRole(IRoleType roleType);

        object GetUnitRole(IRoleType roleType);

        T GetCompositeRole<T>(IRoleType roleType) where T : IObject;

        IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : IObject;

        void SetRole(IRoleType roleType, object value);

        void SetUnitRole(IRoleType roleType, object value);

        void SetCompositeRole(IRoleType roleType, object value);

        void SetCompositesRole(IRoleType roleType, object value);

        void AddRole(IRoleType roleType, IObject value);

        void RemoveRole(IRoleType roleType, IObject value);

        IObject GetCompositeAssociation(IAssociationType associationType);

        IEnumerable<IObject> GetCompositesAssociation(IAssociationType associationType);

        bool CanRead(IRoleType roleType);

        bool CanWrite(IRoleType roleType);

        bool CanExecute(IMethodType methodType);
    }
}
