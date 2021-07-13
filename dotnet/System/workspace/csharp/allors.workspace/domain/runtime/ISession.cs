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

        ISessionServices Services { get; }

        Task<IInvokeResult> Invoke(Method method, InvokeOptions options = null);

        Task<IInvokeResult> Invoke(Method[] methods, InvokeOptions options = null);

        Task<IPullResult> Pull(params Pull[] pulls);

        Task<IPullResult> Call(Procedure procedure, params Pull[] pulls);

        Task<IPushResult> Push();

        T Create<T>() where T : class, IObject;

        T Create<T>(IClass @class) where T : class, IObject;

        IChangeSet Checkpoint();

        #region Instantiate
        T Instantiate<T>(IObject @object) where T : IObject;

        T Instantiate<T>(T @object) where T : IObject;

        T Instantiate<T>(long? id) where T : IObject;

        T Instantiate<T>(long id) where T : IObject;

        T Instantiate<T>(string idAsString) where T : IObject;

        IEnumerable<T> Instantiate<T>(IEnumerable<IObject> objects) where T : IObject;

        IEnumerable<T> Instantiate<T>(IEnumerable<T> objects) where T : IObject;

        IEnumerable<T> Instantiate<T>(IEnumerable<long> ids) where T : IObject;

        IEnumerable<T> Instantiate<T>(IEnumerable<string> ids) where T : IObject;

        IEnumerable<T> Instantiate<T>() where T : IObject;

        IEnumerable<T> Instantiate<T>(IComposite objectType) where T : IObject;
        #endregion
    }
}
