// <copyright file="TestPullController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;

    using Allors.Domain;
    using Server;
    using Allors.Services;
    using Api.Json.Pull;
    using Microsoft.AspNetCore.Mvc;

    public class TestPullController : Controller
    {
        public TestPullController(ISessionService sessionService, IWorkspaceService workspaceService)
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
            try
            {
                var response = new PullResponseBuilder(this.Session, this.WorkspaceService.Name);
                return this.Ok(response.Build());
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
    }
}
