// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using Configuration;
    using Meta;

    public partial class WorkspaceServices : IWorkspaceServices
    {
        public M M { get; private set; }

        public ITime Time { get; private set; }

        public void OnInit(IWorkspace workspace)
        {
            this.M = (M)workspace.Configuration.MetaPopulation;
            this.Time = new Time();
        }

        public void Dispose()
        {
        }

        public ISessionServices CreateSessionServices() => new SessionServices();

        public T Get<T>() =>
           typeof(T) switch
           {
               // Core
               { } type when type == typeof(M) => (T)this.M,
               { } type when type == typeof(ITime) => (T)this.Time,
               _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
           };
    }
}
