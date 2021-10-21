// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using Allors.Database;
using Allors.Database.Adapters.Sql;
using Allors.Database.Configuration;
using Allors.Database.Domain.Tests;
using Allors.Database.Meta;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using src.app.auth;
using src.app.main;
using Database = Allors.Database.Adapters.Sql.SqlClient.Database;
using ObjectFactory = Allors.Database.ObjectFactory;

namespace Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using OpenQA.Selenium;

    public abstract class Test : IDisposable
    {
        public const string ClientUrl = "http://localhost:4200";
        public const string ServerUrl = "http://localhost:5000/allors";

        public static readonly string DatabaseInitUrl = $"{ServerUrl}/Test/Init";
        public static readonly string DatabaseTimeShiftUrl = $"{ServerUrl}/Test/TimeShift";

        private static readonly FileInfo populationFileInfo;
        private static string population;

        static Test()
        {
            var domainPrint = typeof(User).Assembly.Fingerprint();
            var testPrint = typeof(Test).Assembly.Fingerprint();
            var testPopulationPrint = typeof(Marker).Assembly.Fingerprint();
            populationFileInfo = new FileInfo($"population.{domainPrint}.{testPrint}.{testPopulationPrint}.xml");
        }

        protected Test(Fixture fixture)
        {
            this.M = fixture.MetaPopulation;

            // Init Browser
            this.DriverManager = new DriverManager();
            this.DriverManager.Start();

            // Init Server
            this.Driver.Navigate().GoToUrl(Test.DatabaseInitUrl);

            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

            NLog.LogManager.LoadConfiguration("nlog.config");

            // Init Allors
            var configurationBuilder = new ConfigurationBuilder();
            const string root = "/config/apps";
            configurationBuilder.AddCrossPlatform(".");
            configurationBuilder.AddCrossPlatform(root);
            configurationBuilder.AddCrossPlatform(Path.Combine(root, "server"));
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var objectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User));
            var services = new DefaultDatabaseServices(fixture.Engine, null);
            var database = new Database(services, new Configuration
            {
                ObjectFactory = objectFactory,
                ConnectionString = configuration["ConnectionStrings:DefaultConnection"],
            });

            if (population == null && populationFileInfo.Exists)
            {
                population = File.ReadAllText(populationFileInfo.FullName);
            }

            if (population != null)
            {
                using var stringReader = new StringReader(population);
                using var reader = XmlReader.Create(stringReader);
                database.Load(reader);
            }
            else
            {
                database.Init();

                var config = new Config();
                new Setup(database, config).Apply();

                using var transaction = database.CreateTransaction();

                new IntranetPopulation(transaction, null, this.M).Execute();

                transaction.Commit();

                using var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    database.Save(writer);
                }

                population = stringWriter.ToString();
                File.WriteAllText(populationFileInfo.FullName, population);
            }

            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M { get; set; }

        public ILogger Logger { get; set; }

        public ITransaction Transaction { get; set; }

        public DriverManager DriverManager { get; }

        public IWebDriver Driver => this.DriverManager.Driver;

        public Sidenav Sidenav => new MainComponent(this.Driver, this.M).Sidenav;

        public virtual void Dispose() => this.DriverManager.Stop();

        public void Login(string userName = null)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = new UserGroups(this.Transaction).Administrators.Members.First().UserName;
            }

            this.Driver.Navigate().GoToUrl(Test.ClientUrl + "/login");
            var login = new LoginComponent(this.Driver, this.M);
            login.Login(userName);
        }
    }
}
