// <copyright file="Commands.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Data;
    using System.IO;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using McMaster.Extensions.CommandLineUtils;

    using Microsoft.Extensions.Configuration;
    using NLog;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using User = Allors.Database.Domain.User;

    [Command(Description = "Allors Core Commands")]
    [Subcommand(
        typeof(Save),
        typeof(Load),
        typeof(Upgrade),
        typeof(Populate),
        typeof(Print),
        typeof(Custom))]
    public class Program
    {
        private IConfigurationRoot configuration;

        private IDatabase database;

        [Option("-i", Description = "Isolation Level (Snapshot|RepeatableRead|Serializable)")]
        public IsolationLevel? IsolationLevel { get; set; }

        [Option("-t", Description = "Command Timeout in seconds")]
        public int? CommandTimeout { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        public IConfigurationRoot Configuration
        {
            get
            {
                if (this.configuration == null)
                {
                    const string root = "/config/core";

                    var configurationBuilder = new ConfigurationBuilder();

                    configurationBuilder.AddCrossPlatform(".");
                    configurationBuilder.AddCrossPlatform(root);
                    configurationBuilder.AddCrossPlatform(System.IO.Path.Combine(root, "commands"));
                    configurationBuilder.AddEnvironmentVariables();

                    this.configuration = configurationBuilder.Build();
                }

                return this.configuration;
            }
        }

        public DirectoryInfo DataPath => new DirectoryInfo(".").GetAncestorSibling(this.Configuration["datapath"]);

        public IDatabase Database
        {
            get
            {
                if (this.database == null)
                {
                    var metaPopulation = new MetaBuilder().Build();
                    var engine = new Engine(Rules.Create(metaPopulation));
                    var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
                    var databaseBuilder = new DatabaseBuilder(new DefaultDatabaseServices(engine), this.Configuration, objectFactory, this.IsolationLevel, this.CommandTimeout);
                    this.database = databaseBuilder.Build();
                }

                return this.database;
            }
        }

        public MetaPopulation M => this.Database.Services.Get<MetaPopulation>();

        public static int Main(string[] args)
        {
            try
            {
                var app = new CommandLineApplication<Program>();
                app.Conventions.UseDefaultConventions();
                return app.Execute(args);
            }
            catch (Exception e)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error(e, e.Message);
                return ExitCode.Error;
            }
        }
    }
}
