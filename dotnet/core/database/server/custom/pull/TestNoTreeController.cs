// <copyright file="TestNoTreeController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Database.Domain;
    using Services;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Protocol.Json;

    public class TestNoTreeController : Controller
    {
        public TestNoTreeController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = transactionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services().Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public IActionResult Pull()
        {
            var api = new Api(this.Transaction, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();
            response.AddObject("object", api.User);
            response.AddCollection("collection", new Organisations(this.Transaction).Extent());
            return this.Ok(response.Build());
        }
    }
}
