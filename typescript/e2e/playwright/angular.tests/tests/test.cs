// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using OpenQA.Selenium;
    using src.app.auth;
    using src.app.dashboard;
    using src.app.main;
    using C1 = Allors.Database.Domain.C1;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;

    public abstract class Test : IDisposable
    {
        public const string ClientUrl = "http://localhost:4200";
        public const string ServerUrl = "http://localhost:5000/allors";

        public static readonly string DatabaseInithUrl = $"{ServerUrl}/Test/Init";
        public static readonly string DatabaseTimeShiftUrl = $"{ServerUrl}/Test/TimeShift";

        protected Test(Fixture fixture)
        {
            // Start Driver
            this.DriverManager = new DriverManager();
            this.DriverManager.Start();

            // Init Server
            this.Driver.Navigate().GoToUrl(Test.DatabaseInithUrl);

            // Init Allors
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

            const string root = "/config/core";
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCrossPlatform(".");
            configurationBuilder.AddCrossPlatform(root);
            configurationBuilder.AddCrossPlatform(Path.Combine(root, "commands"));
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new Database(
                new DefaultDatabaseServices(engine),
                new Configuration
                {
                    ConnectionString = configuration["ConnectionStrings:DefaultConnection"],
                    ObjectFactory = new Allors.Database.ObjectFactory(metaPopulation, typeof(C1)),
                });

            this.M = database.MetaPopulation as MetaPopulation;

            database.Init();

            var config = new Config();
            new Setup(database, config).Apply();

            this.Transaction = database.CreateTransaction();

            new Population(this.Transaction, null).Execute();

            this.Transaction.Commit();
        }

        public MetaPopulation M { get; set; }

        public IWebDriver Driver => this.DriverManager.Driver;

        public DriverManager DriverManager { get; }

        public ITransaction Transaction { get; set; }

        public Sidenav Sidenav => new MainComponent(this.Driver, this.M).Sidenav;

        public virtual void Dispose() => this.DriverManager.Stop();

        public DashboardComponent Login(string userName = "administrator")
        {
            this.Driver.Navigate().GoToUrl(Test.ClientUrl + "/login");

            var page = new LoginComponent(this.Driver, this.M);
            return page.Login();
        }
    }
}
