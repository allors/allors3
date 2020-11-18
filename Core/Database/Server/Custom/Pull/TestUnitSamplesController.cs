// <copyright file="TestUnitSamplesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Domain;
    using Database.Protocol.Json;

    public class TestUnitSamplesController : Controller
    {
        public TestUnitSamplesController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Session = sessionService.Session;
            this.TreeCache = this.Session.Database.State().TreeCache;
        }

        private ISession Session { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public async Task<IActionResult> Pull([FromBody] TestUnitSamplesParams @params)
        {
            try
            {
                var api = new Api(this.Session, this.WorkspaceService.Name);
                var response = api.CreatePullResponseBuilder();

                var unitSample = this.Session.UnitSample(@params.Step);
                response.AddObject("unitSample", unitSample);
                var pullResponse = response.Build();

                return this.Ok(pullResponse);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        public class TestUnitSamplesParams
        {
            public int Step
            {
                get;
                set;
            }
        }
    }
}
