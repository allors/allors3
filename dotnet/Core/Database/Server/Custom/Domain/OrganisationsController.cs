// <copyright file="OrganisationsController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Database;
    using Database.Domain;
    using Database.Protocol.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Services;

    public class OrganisationsController : Controller
    {
        public OrganisationsController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = transactionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services.Get<ITreeCache>();
        }

        public ITreeCache TreeCache { get; }

        public IWorkspaceService WorkspaceService { get; }

        private ITransaction Transaction { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull(CancellationToken cancellationToken)
        {
            var api = new Api(this.Transaction, this.WorkspaceService.Name, cancellationToken);
            var response = api.CreatePullResponseBuilder();
            var organisations = new Organisations(this.Transaction);
            response.AddCollection("organisations", organisations.ObjectType, organisations.Extent().ToArray());
            return this.Ok(response.Build());
        }
    }
}
