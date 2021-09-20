// <copyright file="DefaultDomainTransactionServices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Http;
    using Services;

    public class TransactionServices : ITransactionServices
    {
        private readonly HttpUserService userService;

        private IDatabaseAclsService databaseAclsService;
        private IWorkspaceAclsService workspaceAclsService;
        private IObjectBuilderService objectBuilderService;

        public TransactionServices(IHttpContextAccessor httpContextAccessor) => this.userService = new HttpUserService(httpContextAccessor);

        public virtual void OnInit(ITransaction transaction)
        {
            this.Transaction = transaction;
            this.userService.OnInit(transaction);
        }

        public ITransaction Transaction { get; private set; }

        public T Get<T>() =>
            typeof(T) switch
            {
                { } type when type == typeof(IUserService) => (T)(IUserService)this.userService,
                { } type when type == typeof(IDatabaseAclsService) => (T)(this.databaseAclsService ??= new DatabaseAclsService(this.userService.User)),
                { } type when type == typeof(IWorkspaceAclsService) => (T)(this.workspaceAclsService ??= new WorkspaceAclsService(this.userService.User)),
                { } type when type == typeof(IObjectBuilderService) => (T)(this.objectBuilderService ??= new ObjectBuilderService(this.Transaction)),
                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };

        public void Dispose()
        {
        }
    }
}