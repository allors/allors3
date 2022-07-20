// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using Configuration;
    using Derivations;
    using Domain;
    using Excel;
    using Meta;

    public partial class WorkspaceServices : IWorkspaceServices
    {
        private readonly IExcelServices excelServices;

        public WorkspaceServices(IExcelServices excelServices = null) => this.excelServices = excelServices;

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
               // Excel
               { } type when type == typeof(IErrorService) => (T)this.excelServices.ErrorService,
               { } type when type == typeof(ILoggerService) => (T)this.excelServices.LoggerService,
               { } type when type == typeof(IMessageService) => (T)this.excelServices.MessageService,
               { } type when type == typeof(IUserIdService) => (T)this.excelServices.UserIdService,
               { } type when type == typeof(IRibbonService) => (T)this.excelServices.RibbonService,
               _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
           };
    }
}
