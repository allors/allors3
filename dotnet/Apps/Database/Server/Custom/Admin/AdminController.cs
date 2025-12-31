// <copyright file="AdminController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Admin;
    using Database;
    using Database.Domain;
    using Database.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Services;

    public class AdminController : Controller
    {
        public AdminController(IDatabaseService databaseService, IConfiguration configuration, ILogger<AdminController> logger)
        {
            this.DatabaseService = databaseService;
            this.Configuration = configuration;
            this.Logger = logger;
        }

        public IDatabaseService DatabaseService { get; set; }

        public IConfiguration Configuration { get; set; }

        public IDatabase Database => this.DatabaseService.Database;

        private ILogger<AdminController> Logger { get; set; }

        private DirectoryInfo DataPath
        {
            get
            {
                var dataPath = this.Configuration["datapath"];
                return !string.IsNullOrEmpty(dataPath) ? new DirectoryInfo(".").GetAncestorSibling(dataPath) : null;
            }
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Save()
        {
            try
            {
                this.Logger.LogInformation("Save: Begin");

                var memoryStream = new MemoryStream();
                using (var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings { CloseOutput = false }))
                {
                    this.Database.Save(writer);
                }

                memoryStream.Position = 0;
                this.Logger.LogInformation("Save: End");
                return this.File(memoryStream, "application/xml", "population.xml");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Save: Exception");
                return this.BadRequest(e.Message);
            }
        }

        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Load(IFormFile file)
        {
            try
            {
                this.Logger.LogInformation("Load: Begin");

                using (var stream = file.OpenReadStream())
                {
                    using (var reader = XmlReader.Create(stream))
                    {
                        this.Database.Load(reader);
                    }
                }

                this.Logger.LogInformation("Load: End");
                return this.Ok();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Load: Exception");
                return this.BadRequest(e.Message);
            }
        }

        public static void Populate(IDatabase database, DirectoryInfo dataPath)
        {
            database.Init();

            var config = new Config { DataPath = dataPath };
            new Setup(database, config).Apply();

            using var transaction = database.CreateTransaction();
            transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

            new TestPopulation(transaction, config).Populate(database);

            transaction.Derive();
            transaction.Commit();

            new Upgrade(transaction, dataPath).Execute();

            transaction.Derive();
            transaction.Commit();
        }

        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Populate()
        {
            try
            {
                this.Logger.LogInformation("Populate: Begin");
                Populate(this.Database, this.DataPath);
                this.Logger.LogInformation("Populate: End");
                return this.Ok();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Populate: Exception");
                return this.BadRequest(e.Message);
            }
        }

        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Upgrade(IFormFile file)
        {
            try
            {
                this.Logger.LogInformation("Upgrade: Begin");

                var notLoadedObjectTypeIds = new HashSet<Guid>();
                var notLoadedRelationTypeIds = new HashSet<Guid>();
                var notLoadedObjects = new HashSet<long>();

                using (var stream = file.OpenReadStream())
                {
                    using (var reader = XmlReader.Create(stream))
                    {
                        this.Database.ObjectNotLoaded += (sender, args) =>
                        {
                            notLoadedObjectTypeIds.Add(args.ObjectTypeId);
                            notLoadedObjects.Add(args.ObjectId);
                        };

                        this.Database.RelationNotLoaded += (sender, args) =>
                        {
                            if (!notLoadedObjects.Contains(args.AssociationId))
                            {
                                notLoadedRelationTypeIds.Add(args.RelationTypeId);
                            }
                        };

                        this.Logger.LogInformation("Upgrade: Loading");
                        this.Database.Load(reader);
                    }
                }

                if (notLoadedObjectTypeIds.Count > 0 || notLoadedRelationTypeIds.Count > 0)
                {
                    var response = new UpgradeResponse
                    {
                        Success = false,
                        ErrorMessage = "Could not load some object or relation types",
                        NotLoadedObjectTypeIds = notLoadedObjectTypeIds.ToArray(),
                        NotLoadedRelationTypeIds = notLoadedRelationTypeIds.ToArray(),
                    };

                    this.Logger.LogError(
                        "Upgrade: Not loaded - ObjectTypes: {ObjectTypes}, RelationTypes: {RelationTypes}",
                        string.Join(", ", notLoadedObjectTypeIds),
                        string.Join(", ", notLoadedRelationTypeIds));

                    return this.BadRequest(response);
                }

                using (var transaction = this.Database.CreateTransaction())
                {
                    this.Database.Services.Get<IPermissions>().Sync(transaction);

                    new Upgrade(transaction, this.DataPath).Execute();
                    transaction.Commit();

                    new Security(transaction).Apply();
                    transaction.Commit();
                }

                this.Logger.LogInformation("Upgrade: End");
                return this.Ok(new UpgradeResponse { Success = true });
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Upgrade: Exception");
                return this.BadRequest(new UpgradeResponse
                {
                    Success = false,
                    ErrorMessage = e.Message,
                });
            }
        }
    }
}
