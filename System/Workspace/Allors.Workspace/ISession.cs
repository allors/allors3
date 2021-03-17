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

        ISessionLifecycle SessionLifecycle { get; }

        T Create<T>() where T : class, IObject;

        T Create<T>(IClass @class) where T : IObject;

        T Get<T>(IObject @object) where T : IObject;

        T Get<T>(T @object) where T : IObject;

        T Get<T>(long identity) where T : IObject;

        IEnumerable<T> Get<T>(IEnumerable<IObject> objects) where T : IObject;

        IEnumerable<T> Get<T>(IEnumerable<T> objects) where T : IObject;

        IEnumerable<T> Get<T>(IEnumerable<long> identities) where T : IObject;

        IEnumerable<T> GetAll<T>() where T : class, IObject;

        IEnumerable<T> GetAll<T>(IComposite objectType) where T : IObject;

        void Reset();

        void Merge();

        Task<ICallResult> Call(Method method, CallOptions options = null);

        Task<ICallResult> Call(Method[] methods, CallOptions options = null);

        Task<ILoadResult> Load(params Pull[] pulls);

        Task<ILoadResult> Load(string service, object args);

        Task<ISaveResult> Save();

        IChangeSet Checkpoint();
    }
}
