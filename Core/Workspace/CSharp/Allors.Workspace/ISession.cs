// <copyright file="IDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Threading.Tasks;
    using Data;
    using Meta;
    using System.Collections.Generic;

    public interface ISession
    {
        IWorkspace Workspace { get; }

        ISessionLifecycle StateLifecycle { get; }

        T Create<T>() where T : class, IObject;

        IObject Create(IClass @class);

        T Instantiate<T>(T @object) where T : IObject;

        IObject Instantiate(long id);

        IEnumerable<IObject> Instantiate(IEnumerable<long> ids);

        void Reset();

        void Refresh(bool merge = false);

        Task<ICallResult> Call(Method method, CallOptions options = null);

        Task<ICallResult> Call(Method[] methods, CallOptions options = null);

        Task<ICallResult> Call(string service, object args);

        Task<ILoadResult> Load(params Pull[] pulls);

        Task<ILoadResult> Load(string service, object args);

        Task<ISaveResult> Save();
    }
}
