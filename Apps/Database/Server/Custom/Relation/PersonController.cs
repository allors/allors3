// <copyright file="PersonController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Services;
    using Api.Json.Pull;
    using Microsoft.AspNetCore.Mvc;

    public class PersonController : Controller
    {
        public PersonController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.SessionService = sessionService;
            this.WorkspaceService = workspaceService;
        }

        public ISessionService SessionService { get; }

        public IWorkspaceService WorkspaceService { get; }


        [HttpPost]
        public IActionResult Pull([FromBody] Model model)
        {
            var response = new PullResponseBuilder(this.SessionService.Session, this.WorkspaceService.Name);

            var person = this.SessionService.Session.Instantiate(model.Id);
            response.AddObject("person", person);

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}