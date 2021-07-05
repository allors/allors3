// <copyright file="DefaultTransactionContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using System;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules.Default;
using Allors.Database.Services;

namespace Allors.Database.Configuration
{
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Http;

    public class DefaultTransactionServices : IDomainTransactionServices
    {
        private readonly HttpContext httpContext;

        private IDeriveService deriveService;
        private IDatabaseAclsService databaseAclsService;
        private IWorkspaceAclsService workspaceAclsService;
        private IObjectBuilderService objectBuilderService;

        public DefaultTransactionServices(IHttpContextAccessor httpContextAccessor) => this.httpContext = new HttpContext(httpContextAccessor);

        public virtual void OnInit(ITransaction transaction)
        {
            this.Transaction = transaction;
            this.httpContext.OnInit(transaction);
        }

        public ITransaction Transaction { get; private set; }

        public User User
        {
            get => this.httpContext.User;
            set => this.httpContext.User = value;
        }

        public T Get<T>() =>
            typeof(T).Name switch
            {
                nameof(IDeriveService) => (T)(this.deriveService ??= new DefaultDeriveService(this.Transaction)),
                nameof(IDatabaseAclsService) => (T)(this.databaseAclsService ??= new DatabaseAclsService(this.User)),
                nameof(IWorkspaceAclsService) => (T)(this.workspaceAclsService ??= new WorkspaceAclsService(this.User)),
                nameof(IObjectBuilderService) => (T)(this.objectBuilderService ??= new ObjectBuilderService(this.Transaction)),

                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };
        public void Dispose()
        {
        }
    }
}