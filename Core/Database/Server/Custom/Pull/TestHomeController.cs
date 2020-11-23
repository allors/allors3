// <copyright file="TestHomeController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Database.Domain;
    using Services;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Data;
    using Database.Protocol.Json;

    public class TestHomeController : Controller
    {
        public TestHomeController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.Context().TreeCache;
        }

        private ISession Session { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public IActionResult Pull()
        {
            var api = new Api(this.Session, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();

            var m = this.Session.Database.Context().M;
            var organisation = new Organisations(this.Session).FindBy(m.Organisation.Owner, this.Session.Context().User);
            response.AddObject("root", organisation, new[] {
                new Node(m.Organisation.Shareholders)
            });
            return this.Ok(response.Build());
        }
    }
}
