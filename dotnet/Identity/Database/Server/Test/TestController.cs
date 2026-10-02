// <copyright file="TestController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using Database;
    using Database.Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;

    [AllowAnonymous]
    public partial class TestController : Controller
    {
        private readonly ILogger<TestController> logger;

        public TestController(IDatabaseService databaseService, ILogger<TestController> logger)
        {
            this.DatabaseService = databaseService;
            this.logger = logger;
        }

        public IDatabaseService DatabaseService { get; set; }

        public IDatabase Database => this.DatabaseService.Database;

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Ready() => this.Ok();

        // Resets the database to the test population of this tree: the setup of the Identity test
        // domain, plus jane@example.com as its administrator, whom the tests know by user name.
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Setup()
        {
            try
            {
                var database = this.Database;
                database.Init();

                new Setup(database, new Config()).Apply();

                using (var transaction = database.CreateTransaction())
                {
                    transaction.Derive();
                    transaction.Commit();

                    var jane = new PersonBuilder(transaction).WithUserName("jane@example.com").Build();
                    new UserGroups(transaction).Administrators.AddMember(jane);

                    transaction.Derive();
                    transaction.Commit();
                }

                return this.Ok();
            }
            catch (Exception e)
            {
                this.LogActionFailed(e, nameof(this.Setup));
                return this.BadRequest(e.Message);
            }
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Test action {Action} failed")]
        private partial void LogActionFailed(Exception exception, string action);
    }
}
