// <copyright file="TestHomeController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Domain;
    using Allors.Services;
    using Api.Json.Pull;
    using Data;
    using Microsoft.AspNetCore.Mvc;
    using Allors.State;

    public class TestHomeController : Controller
    {
        public TestHomeController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.State().TreeCache;
        }

        private ISession Session { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public IActionResult Pull()
        {
            var m = this.Session.Database.State().M;
            var response = new PullResponseBuilder(this.Session, this.WorkspaceService.Name);
            var organisation = new Organisations(this.Session).FindBy(m.Organisation.Owner, this.Session.State().User);
            response.AddObject("root", organisation, new[] {
                new Node(m.Organisation.Shareholders)
            });
            return this.Ok(response.Build());
        }
    }
}
