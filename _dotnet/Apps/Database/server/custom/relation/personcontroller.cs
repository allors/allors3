// <copyright file="PersonController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using Allors.Services;
    using Services;
    using Protocol.Json;
    using Microsoft.AspNetCore.Mvc;

    public class PersonController : Controller
    {
        public PersonController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.TransactionService = transactionService;
            this.WorkspaceService = workspaceService;
        }

        public ITransactionService TransactionService { get; }

        public IWorkspaceService WorkspaceService { get; }


        [HttpPost]
        public IActionResult Pull([FromBody] Model model)
        {
            var api = new Api(this.TransactionService.Transaction, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();

            var person = this.TransactionService.Transaction.Instantiate(model.Id);
            response.AddObject("person", person);

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}
