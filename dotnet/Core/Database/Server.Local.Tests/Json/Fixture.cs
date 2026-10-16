// <copyright file="Fixture.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;

    public class Fixture : IDisposable
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private string connectionString;

        public Fixture()
        {
            this.MetaPopulation = MetaBuilder.Build();
            var rules = Rules.Create(this.MetaPopulation);
            this.Engine = new Engine(rules);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAllorsConfiguration("core", "commands");
            this.Configuration = configurationBuilder.Build();
            this.Adapter = this.Configuration["adapter"];
        }

        public MetaPopulation MetaPopulation { get; set; }

        public Engine Engine { get; set; }

        public IConfigurationRoot Configuration { get; }

        public string Adapter { get; }

        /// <summary>
        /// Drops and recreates this test class's database (once for the class) from the admin connection,
        /// and returns the connection string to use against it.
        /// </summary>
        public string EnsureDatabase(string database)
        {
            if (this.connectionString == null)
            {
                var prefix = Environment.GetEnvironmentVariable("ALLORS_TEST_DATABASE_PREFIX");
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(prefix, @"\A[a-z][a-z0-9_]*\z") || prefix.Length + database.Length > 63)
                    {
                        throw new ArgumentException("Set ALLORS_TEST_DATABASE_PREFIX to lowercase ASCII letters, digits and underscores, starting with a letter, with at most 63 characters including the test class name.");
                    }

                    database = prefix + database;
                }

                DatabaseProvisioning.DropCreate(this.Adapter, database);
                this.connectionString = DatabaseProvisioning.ConnectionString(this.Adapter, database);
            }

            return this.connectionString;
        }

        public void Dispose() => this.MetaPopulation = null;
    }
}
