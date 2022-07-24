// <copyright file="PeopleController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Services;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Protocol.Json;


    public class PeopleController : Controller
    {
        public PeopleController(ITransactionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = sessionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services.Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull(CancellationToken cancellationToken)
        {
            var api = new Api(this.Transaction, this.WorkspaceService.Name, cancellationToken);
            var response = api.CreatePullResponseBuilder();
            var people = new People(this.Transaction);
            response.AddCollection("people", people.ObjectType, people.Extent());
            return this.Ok(response.Build());
        }
    }
}
