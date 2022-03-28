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
using Database = Allors.Database.Adapters.Sql.SqlClient.Database;
using ObjectFactory = Allors.Database.ObjectFactory;

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Allors.Database.Domain;
    using Allors.E2E.Angular.Material.Authentication;
    using Microsoft.Playwright;
    using NUnit.Framework;


    public abstract class Test : E2ETest
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
            var testPopulationPrint = typeof(IntranetPopulation).Assembly.Fingerprint();
            populationFileInfo = new FileInfo($"population.{domainPrint}.{testPrint}.{testPopulationPrint}.xml");
        }

        [SetUp]
        public async System.Threading.Tasks.Task TestSetup()
        {
            this.Playwright = await PlaywrightFactory;
            this.BrowserType = this.Playwright[(Environment.GetEnvironmentVariable("BROWSER") ?? Microsoft.Playwright.BrowserType.Chromium).ToLower()];

            var browserTypeLaunchOptions = new BrowserTypeLaunchOptions();
            if (bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var headless))
            {
                browserTypeLaunchOptions.Headless = headless;
            }

            this.Configure(browserTypeLaunchOptions);
            this.Browser = await this.BrowserType.LaunchAsync(browserTypeLaunchOptions);
            var browserNewContextOptions = new BrowserNewContextOptions();
            this.Configure(browserNewContextOptions);
            this.Context = await this.Browser.NewContextAsync(browserNewContextOptions);
            this.Page = await this.Context.NewPageAsync();

            this.ConsoleMessages = new List<IConsoleMessage>();
            this.Page.Console += (_, message) => this.ConsoleMessages.Add(message);

            this.M = this.Fixture.MetaPopulation;
            this.Database = this.Fixture.Init();
            this.Transaction = this.Database.CreateTransaction();

            // Population
            if (population == null && populationFileInfo.Exists)
            {
                population = File.ReadAllText(populationFileInfo.FullName);
            }

            if (population != null)
            {
                using var stringReader = new StringReader(population);
                using var reader = XmlReader.Create(stringReader);
                this.Database.Load(reader);
            }
            else
            {
                this.Database.Init();

                var config = new Config();
                new Setup(this.Database, config).Apply();

                using var transaction = this.Database.CreateTransaction();

                new IntranetPopulation(transaction, null, this.M).Execute();

                transaction.Commit();

                using var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    this.Database.Save(writer);
                }

                population = stringWriter.ToString();
                File.WriteAllText(populationFileInfo.FullName, population);
            }
        }

        [TearDown]
        public async System.Threading.Tasks.Task TestTearDown()
        {
            await this.Page.CloseAsync();
            await this.Context.CloseAsync();
            await this.Browser.CloseAsync();
        }
    }
}
