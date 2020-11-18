// <copyright file="OrganisationsController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading.Tasks;

    using Allors.Database.Domain;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Protocol.Json;
    

    public class OrganisationsController : Controller
    {
        public OrganisationsController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.Context().TreeCache;
        }

        public ITreeCache TreeCache { get; }

        public IWorkspaceService WorkspaceService { get; }

        private ISession Session { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull()
        {
            var api = new Api(this.Session, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();
            var organisations = new Organisations(this.Session).Extent().ToArray();
            response.AddCollection("organisations", organisations);
            return this.Ok(response.Build());
        }
    }
}
