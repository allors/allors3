// <copyright file="TestUnitSamplesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Allors.Domain;
    using Server;
    using Allors.Services;
    using Api.Json.Pull;
    using Microsoft.AspNetCore.Mvc;

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
                var unitSample = new UnitSamples(this.Session).Extent().First;
                if (unitSample == null)
                {
                    unitSample = new UnitSampleBuilder(this.Session).Build();
                    this.Session.Commit();
                }

                var acls = new WorkspaceAccessControlLists(WorkspaceService.Name, this.Session.State().User);
                var responseBuilder = new PullResponseBuilder(acls, this.TreeCache);

                switch (@params.Step)
                {
                    case 0:
                        unitSample.RemoveAllorsBinary();
                        unitSample.RemoveAllorsBoolean();
                        unitSample.RemoveAllorsDateTime();
                        unitSample.RemoveAllorsDecimal();
                        unitSample.RemoveAllorsDouble();
                        unitSample.RemoveAllorsInteger();
                        unitSample.RemoveAllorsString();
                        unitSample.RemoveAllorsUnique();

                        break;

                    case 1:
                        unitSample.AllorsBinary = new byte[] { 1, 2, 3 };
                        unitSample.AllorsBoolean = true;
                        unitSample.AllorsDateTime = new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc);
                        unitSample.AllorsDecimal = 12.34m;
                        unitSample.AllorsDouble = 123d;
                        unitSample.AllorsInteger = 1000;
                        unitSample.AllorsString = "a string";
                        unitSample.AllorsUnique = new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF");

                        break;
                }

                this.Session.Commit();

                responseBuilder.AddObject("unitSample", unitSample);
                var pullResponse = responseBuilder.Build();

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
