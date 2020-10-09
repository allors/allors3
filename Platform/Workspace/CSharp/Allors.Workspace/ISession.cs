// <copyright file="IDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Protocol.Database.Invoke;
    using Protocol.Database.Push;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;

    public interface ISession
    {
        IWorkspace Workspace { get; }

        ISessionStateLifecycle StateLifecycle { get; }

        ISessionObject Get(long id);

        IEnumerable<ISessionObject> GetAssociation(ISessionObject @object, IAssociationType associationType);

        ISessionObject Create(IClass @class);

        void Reset();

        Task<InvokeResponse> Invoke(Method method, InvokeOptions options = null);

        Task<InvokeResponse> Invoke(Method[] methods, InvokeOptions options = null);

        Task<InvokeResponse> Invoke(string service, object args);

        Task<Result> Load(params Pull[] pulls);

        Task<Result> Load(object args, string pullService = null);

        Task<PushResponse> Save();
    }
}
