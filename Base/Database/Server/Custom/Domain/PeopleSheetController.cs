// <copyright file="PeopleSheetController.cs" company="Allors bvba">
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
    

    public class PeopleSheetController : Controller
    {
        public PeopleSheetController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.Context().TreeCache;
        }

        private ISession Session { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull()
        {
            var api = new Api(this.Session, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();
            var people = new People(this.Session).Extent().ToArray();
            response.AddCollection("people", people);
            return this.Ok(response.Build());
        }
    }
}
