// <copyright file="TestController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using Database;
    using Database.Domain;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;

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
        public IActionResult Ready() => this.Ok();

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Init()
        {
            try
            {
                var database = this.Database;
                database.Init();

                return this.Ok();
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

                var config = new Config();
                new Setup(database, config).Apply();

                using (var transaction = database.CreateTransaction())
                {
                    transaction.Derive();
                    transaction.Commit();

                    var administrator = new PersonBuilder(transaction).WithUserName("administrator").Build();
                    new UserGroups(transaction).Administrators.AddMember(administrator);
                    transaction.Services.Get<IUserService>().User = administrator;

                    new TestPopulation(transaction).Apply();
                    transaction.Derive();
                    transaction.Commit();
                }

                return this.Ok();
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
                var timeService = this.Database.Services.Get<ITime>();
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
