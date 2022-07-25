// <copyright file="TestHomeController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading;
    using Allors.Services;
    using Domain;
    using Data;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Meta;
    using Protocol.Json;
    

    public class TestHomeController : Controller
    {
        public TestHomeController(ITransactionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = sessionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services.Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public IActionResult Pull(CancellationToken cancellationToken)
        {
            var api = new Api(this.Transaction, this.WorkspaceService.Name, cancellationToken);
            var response = api.CreatePullResponseBuilder();

            var m = this.Transaction.Database.Services.Get<MetaPopulation>();
            var organisation = new Organisations(this.Transaction).FindBy(m.Organisation.Owner, this.Transaction.Services.Get<IUserService>().User);
            response.AddObject("root", organisation, new[] {
                new Node(m.Organisation.Shareholders)
                    .Add(m.Person.Photo),
            });
            return this.Ok(response.Build());
        }
    }
}
