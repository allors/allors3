// <copyright file="TestController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using Database;
    using Database.Domain;
    using Database.Services;
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
                this.LogActionFailed(e, nameof(this.Init));
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

                    var administrator = new PersonBuilder(transaction).WithUserName("administrator").WithUniqueId(Users.AdministratorId).Build();
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
                this.LogActionFailed(e, nameof(this.Setup));
                return this.BadRequest(e.Message);
            }
        }

        // The two security changes the workspace tests make between two pulls, so that a grant or a
        // revocation changes version on the server; see TestSecurity.
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult RemoveAdministratorPermission(string relationType, string operation)
        {
            try
            {
                using var transaction = this.Database.CreateTransaction();
                TestSecurity.RemoveAdministratorPermission(transaction, relationType, Enum.Parse<Operations>(operation, true));
                return this.Ok();
            }
            catch (Exception e)
            {
                this.LogActionFailed(e, nameof(this.RemoveAdministratorPermission));
                return this.BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult DenyPermission(string relationType, string operation)
        {
            try
            {
                using var transaction = this.Database.CreateTransaction();
                TestSecurity.DenyPermission(transaction, relationType, Enum.Parse<Operations>(operation, true));
                return this.Ok();
            }
            catch (Exception e)
            {
                this.LogActionFailed(e, nameof(this.DenyPermission));
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
                this.LogActionFailed(e, nameof(this.TimeShift));
                return this.BadRequest(e.Message);
            }
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Test action {Action} failed")]
        private partial void LogActionFailed(Exception exception, string action);
    }
}
