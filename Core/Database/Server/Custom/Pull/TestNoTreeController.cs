// <copyright file="TestNoTreeController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Database.Domain;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Protocol.Json;

    public class TestNoTreeController : Controller
    {
        public TestNoTreeController(ISessionService sessionService, IWorkspaceService workspaceService)
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
            response.AddObject("object", api.User);
            response.AddCollection("collection", new Organisations(this.Session).Extent());
            return this.Ok(response.Build());
        }
    }
}
