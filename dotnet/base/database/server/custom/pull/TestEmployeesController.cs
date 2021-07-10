// <copyright file="TestEmployeesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using Allors.Services;
    using Domain;
    using Data;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Protocol.Json;


    public class TestEmployeesController : Controller
    {
        public TestEmployeesController(ITransactionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = sessionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services().Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public ITreeCache TreeCache { get; }

        public IWorkspaceService WorkspaceService { get; }

        [HttpPost]
        public IActionResult Pull()
        {
            var api = new Api(this.Transaction, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();

            var m = this.Transaction.Database.Services().M;
            var organisation = new Organisations(this.Transaction).FindBy(m.Organisation.Owner, this.Transaction.Services().User);

            response.AddObject("root", organisation, new[]
            {
                new Node(m.Organisation.Employees),
            });

            return this.Ok(response.Build());
        }
    }
}