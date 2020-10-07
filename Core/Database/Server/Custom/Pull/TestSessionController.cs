// <copyright file="TestSessionController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Domain;
    using Allors.Services;
    using Allors.State;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class TestSessionController : Controller
    {
        public TestSessionController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.State().TreeCache;
        }

        private ISession Session { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public IActionResult UserName()
        {
            var user = this.Session.State().User;
            var result = user?.UserName;
            return this.Content(result);
        }
    }
}
