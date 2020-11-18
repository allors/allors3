// <copyright file="TestController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System;
    using Allors.Database.Domain;
    using Allors.Services;
    using Database;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class TestController : Controller
    {
        public TestController(IDatabaseService databaseService, ILogger<TestController> logger)
        {
            this.DatabaseService = databaseService;
            this.Logger = logger;
        }

        public IDatabaseService DatabaseService { get; set; }

        public IDatabase Database => this.DatabaseService.Database;

        private ILogger<TestController> Logger { get; set; }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Ready() => this.Ok("Ready");

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Init()
        {
            try
            {
                var database = this.Database;
                database.Init();

                return this.Ok("Init");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return this.BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Setup(string population)
        {
            try
            {
                var database = this.Database;
                database.Init();

                using (var session = database.CreateSession())
                {
                    var config = new Config();
                    new Setup(session, config).Apply();
                    session.Derive();
                    session.Commit();

                    var administrator = new PersonBuilder(session).WithUserName("administrator").Build();
                    new UserGroups(session).Administrators.AddMember(administrator);
                    session.Context().User = administrator;

                    new TestPopulation(session, population).Apply();
                    session.Derive();
                    session.Commit();
                }

                return this.Ok("Setup");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return this.BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TimeShift(int days, int hours = 0, int minutes = 0, int seconds = 0)
        {
            try
            {
                var timeService = this.Database.Context().Time;
                timeService.Shift = new TimeSpan(days, hours, minutes, seconds);
                return this.Ok();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return this.BadRequest(e.Message);
            }
        }
    }
}
