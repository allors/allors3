// <copyright file="OrganisationsController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System.Threading.Tasks;

    using Allors.Domain;
    using Server;
    using Allors.Services;
    using Api.Json.Pull;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Allors.State;

    public class OrganisationsController : Controller
    {
        public OrganisationsController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.State().TreeCache;
        }

        public ITreeCache TreeCache { get; }

        public IWorkspaceService WorkspaceService { get; }

        private ISession Session { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull()
        {
            var response = new PullResponseBuilder(this.Session, this.WorkspaceService.Name);
            var people = new Organisations(this.Session).Extent().ToArray();
            response.AddCollection("organisations", people, true);
            return this.Ok(response.Build());
        }
    }
}
