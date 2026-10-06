// <copyright file="Program.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Data;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using McMaster.Extensions.CommandLineUtils;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using User = Allors.Database.Domain.User;

    [Command(Description = "Allors Entra Commands")]
    [Subcommand(
        typeof(Save),
        typeof(Load),
        typeof(Populate),
        typeof(Init)
        )]
    public partial class Program
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
                    var configurationBuilder = new ConfigurationBuilder();

                    configurationBuilder.AddAllorsConfiguration("entra", "commands");

                    this.configuration = configurationBuilder.Build();
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
                    var databaseBuilder = new DatabaseBuilder(new DefaultDatabaseServices(engine), this.Configuration, objectFactory, this.IsolationLevel, this.CommandTimeout);
                    this.database = databaseBuilder.Build();
                }

                return this.database;
            }
        }

        public MetaPopulation M => this.Database.Services.Get<Allors.Database.Meta.MetaPopulation>();

        public static int Main(string[] args)
        {
            using var services = new ServiceCollection()
                .AddLogging(builder => builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }))
                .BuildServiceProvider();

            try
            {
                var app = new CommandLineApplication<Program>();
                app.Conventions
                    .UseDefaultConventions()
                    .UseConstructorInjection(services);
                return app.Execute(args);
            }
            catch (Exception e)
            {
                LogFailed(services.GetRequiredService<ILogger<Program>>(), e, e.Message);
                return ExitCode.Error;
            }
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "{ErrorMessage}")]
        private static partial void LogFailed(ILogger logger, Exception exception, string errorMessage);
    }
}
