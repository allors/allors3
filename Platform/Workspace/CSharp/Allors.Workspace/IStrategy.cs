// <copyright file="SessionObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Protocol.Database.Push;
    using Allors.Workspace.Meta;

    public interface IStrategy
    {
        ISessionObject SessionObject { get; }

        long Id { get; }

        long? NewId { get; set; }

        long? Version { get; }

        IClass ObjectType { get; }

        ISession Session { get; }

        bool HasChanges { get; }

        bool CanRead(IRoleType roleType);

        bool CanWrite(IRoleType roleType);

        bool CanExecute(IMethodType methodType);

        bool Exist(IRoleType roleType);

        object Get(IRoleType roleType);

        void Set(IRoleType roleType, object value);

        void Add(IRoleType roleType, ISessionObject value);

        void Remove(IRoleType roleType, ISessionObject value);

        T GetAssociation<T>(IAssociationType associationType);

        T[] GetAssociations<T>(IAssociationType associationType);

        PushRequestObject Save();

        PushRequestNewObject SaveNew();

        void Reset();

        void Refresh(bool merge = false);

        object GetForAssociation(IRoleType roleType);

        void PushResponse(long id);
    }
}
