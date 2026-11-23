// <copyright file="AddUserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using System.Linq;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Xunit;
    using MetaPopulation = Allors.Database.Meta.MetaPopulation;

    // The commands run on one Program, which keeps the population of the Snapshot adapter between them. Without a
    // configuration root the tool reads its settings from the environment.
    public sealed class AddUserTests : IDisposable
    {
        private const string AdapterVariable = "adapter";

        private readonly string configRoot = Environment.GetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable);
        private readonly string adapter = Environment.GetEnvironmentVariable(AdapterVariable);

        public AddUserTests()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, null);
            Environment.SetEnvironmentVariable(AdapterVariable, "snapshot");
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(ConfigurationBuilderExtensions.ConfigRootVariable, this.configRoot);
            Environment.SetEnvironmentVariable(AdapterVariable, this.adapter);
        }

        [Fact]
        public void UserCreatesAUserWithThePasswordInTheUserGroup()
        {
            var program = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = program }.OnExecute(null));

            var addUser = new AddUser { Parent = program, Name = "jane@example.com", Password = "correct horse battery", Group = "Workers" };
            Assert.Equal(ExitCode.Success, addUser.OnExecute(null));

            var m = program.Database.Services.Get<MetaPopulation>();
            using var transaction = program.Database.CreateTransaction();
            var jane = (Person)new Users(transaction).FindBy(m.User.UserName, "jane@example.com");
            Assert.NotNull(jane);
            Assert.True(jane.VerifyPassword("correct horse battery"));
            Assert.False(jane.VerifyPassword("another horse battery"));
            Assert.Contains(jane, new UserGroups(transaction).Workers.Members);
        }

        [Theory]
        [InlineData("Cleaners", "correct horse battery", "jane@example.com")]
        [InlineData("Workers", "too short", "jane@example.com")]
        [InlineData("Workers", "correct horse battery", "john@example.com")]
        [InlineData("Workers", "correct horse battery", "John@Example.com")]
        public void UserCreatesNoUserForAUserGroupThatDoesNotExistAWeakPasswordOrAUserNameThatExists(string group, string password, string name)
        {
            var program = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = program }.OnExecute(null));
            Assert.Equal(ExitCode.Success, new AddUser { Parent = program, Name = "john@example.com", Password = "correct horse battery", Group = "Workers" }.OnExecute(null));

            int userCount;
            using (var transaction = program.Database.CreateTransaction())
            {
                userCount = new Users(transaction).Extent().Count;
            }

            Assert.Equal(ExitCode.Error, new AddUser { Parent = program, Name = name, Password = password, Group = group }.OnExecute(null));

            using (var transaction = program.Database.CreateTransaction())
            {
                Assert.Equal(userCount, new Users(transaction).Extent().Count);
                Assert.Single(new Users(transaction).Extent().ToArray(), v => v.UserName == "john@example.com");
            }
        }
    }
}
