// <copyright file="Program.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
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

    // The commands save, load and index are Core's, and ask this class for the configuration and the
    // database. The commands init, populate, user and upgrade are the sample's own, in Custom.
    [Command(Description = "The commands of the sample application")]
    [Subcommand(
        typeof(Init),
        typeof(Populate),
        typeof(AddUser),
        typeof(Save),
        typeof(Load),
        typeof(Upgrade),
        typeof(Index))]
    public class Program
    {
        private IConfigurationRoot configuration;
        private IDatabase database;

        [Option("-t", Description = "Command Timeout in seconds")]
        public int? CommandTimeout { get; set; }

        // The folder custom of the configuration root holds the settings of the sample's domain, and its
        // folder commands those of this tool. Environment variables override both.
        public IConfigurationRoot Configuration
        {
            get
            {
                if (this.configuration == null)
                {
                    var configurationBuilder = new ConfigurationBuilder();
                    configurationBuilder.AddAllorsConfiguration("custom", "commands");
                    var configuration = configurationBuilder.Build();
                    configuration.Require("adapter");
                    if (DatabaseBuilder.HasDatabaseServer(configuration["adapter"]))
                    {
                        configuration.Require("ConnectionStrings:DefaultConnection");
                    }

                    this.configuration = configuration;
                }

                return this.configuration;
            }
        }

        public IDatabase Database
        {
            get
            {
                if (this.database == null)
                {
                    var metaPopulation = new MetaBuilder().Build();
                    var engine = new Engine(Rules.Create(metaPopulation));
                    var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
                    var databaseBuilder = new DatabaseBuilder(new DefaultDatabaseServices(engine), this.Configuration, objectFactory, this.CommandTimeout);
                    this.database = databaseBuilder.Build();
                }

                return this.database;
            }
        }

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

        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return ExitCode.Error;
        }
    }
}
