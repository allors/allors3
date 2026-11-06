// <copyright file="DatabaseBuilderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Adapters;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class DatabaseBuilderTests
    {
        [Fact]
        public void BuildNamesTheAdapterWithoutAValue()
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=core",
            });

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'adapter' has no value", exception.Message);
            Assert.Contains("'sqlclient' or 'npgsql'", exception.Message);
        }

        [Fact]
        public void BuildNamesTheUnknownAdapter()
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["adapter"] = "mssql",
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=core",
            });

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'adapter' is 'mssql'", exception.Message);
            Assert.Contains("'sqlclient' or 'npgsql'", exception.Message);
        }

        [Theory]
        [InlineData("sqlclient")]
        [InlineData("npgsql")]
        public void BuildNamesTheConnectionStringWithoutAValue(string adapter)
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["adapter"] = adapter,
            });

            var exception = Assert.Throws<InvalidOperationException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'ConnectionStrings:DefaultConnection' has no value", exception.Message);
        }

        private static DatabaseBuilder Builder(Dictionary<string, string> settings) =>
            new DatabaseBuilder(null, new ConfigurationBuilder().AddInMemoryCollection(settings).Build(), null);
    }
}
