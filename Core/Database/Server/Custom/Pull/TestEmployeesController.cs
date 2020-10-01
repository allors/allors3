// <copyright file="TestEmployeesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Domain;
    using Allors.Meta;
    using Server;
    using Allors.Services;
    using Api.Json.Pull;
    using Data;
    using Microsoft.AspNetCore.Mvc;

    public class TestEmployeesController : Controller
    {
        public TestEmployeesController(ISessionService sessionService)
        {
            this.Session = sessionService.Session;
            this.TreeService = this.Session.Database.Scope().TreeService;
        }

        private ISession Session { get; }

        public ITreeService TreeService { get; }

        [HttpPost]
        public IActionResult Pull()
        {
            var m = ((IDatabaseScope) this.Session.Database.Scope()).M;

            var acls = new WorkspaceAccessControlLists(this.Session.Scope().User);
            var response = new PullResponseBuilder(acls, this.TreeService);
            var organisation = new Organisations(this.Session).FindBy(m.Organisation.Owner, this.Session.Scope().User);

            response.AddObject("root", organisation, new[]
            {
                new Node(m.Organisation.Employees),
            });

            return this.Ok(response.Build());
        }
    }
}
