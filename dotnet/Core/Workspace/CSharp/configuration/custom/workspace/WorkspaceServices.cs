// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using Configuration;
    using Configuration.Derivations.Default;
    using Derivations;
    using Domain;
    using Meta;

    public partial class WorkspaceServices : IWorkspaceServices
    {
        public M M { get; private set; }

        public IDerivationService DerivationService { get; private set; }

        public ITime Time { get; private set; }

        public void OnInit(IWorkspace workspace)
        {
            this.M = (M)workspace.Configuration.MetaPopulation;

            var engine = new Engine(this.CreateRules());
            this.DerivationService = new DerivationService(engine);

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
               { } type when type == typeof(IDerivationService) => (T)this.DerivationService,
               _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
           };

        private IRule[] CreateRules()
        {
            var m = this.M;

            return new IRule[]
            {
                new PersonSessionFullNameRule(m)
            };
        }
    }
}
