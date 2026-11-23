// <copyright file="AddUserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
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

            const string password = " correct horse battery ";
            Assert.Equal(ExitCode.Success, RunUser(program, password + "\nsecond line", "-n", "jane@example.com", "-g", "Workers", "--password-stdin"));

            var m = program.Database.Services.Get<MetaPopulation>();
            using var transaction = program.Database.CreateTransaction();
            var jane = (Person)new Users(transaction).FindBy(m.User.UserName, "jane@example.com");
            Assert.NotNull(jane);
            Assert.True(jane.UserLockoutEnabled);
            Assert.True(jane.VerifyPassword(password));
            Assert.False(jane.VerifyPassword("another horse battery"));
            Assert.Contains(jane, new UserGroups(transaction).Workers.Members);
        }

        [Fact]
        public void UserGroupsWithTheSameNameCanOnlyBeSelectedById()
        {
            var program = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = program }.OnExecute(null));

            Guid secondGroupId;
            using (var transaction = program.Database.CreateTransaction())
            {
                var secondGroup = new UserGroupBuilder(transaction).WithName("Workers").Build();
                transaction.Derive();
                transaction.Commit();
                secondGroupId = secondGroup.UniqueId;
            }

            const string password = "correct horse battery";
            const string name = "jane@example.com";
            Assert.Equal(ExitCode.Error, RunUser(program, password, "-n", name, "--group", "Workers", "--password-stdin"));
            Assert.Equal(ExitCode.Success, RunUser(program, password, "-n", name, "--group-id", secondGroupId.ToString(), "--password-stdin"));

            var m = program.Database.Services.Get<MetaPopulation>();
            using var check = program.Database.CreateTransaction();
            var jane = new Users(check).FindBy(m.User.UserName, name);
            Assert.NotNull(jane);
            var groups = new UserGroups(check);
            Assert.Contains(jane, groups.FindBy(m.UserGroup.UniqueId, secondGroupId).Members);
            Assert.DoesNotContain(jane, groups.Workers.Members);
        }

        [Theory]
        [InlineData(new[] { "--group", "Cleaners" }, "correct horse battery", "jane@example.com")]
        [InlineData(new string[] { }, "correct horse battery", "jane@example.com")]
        [InlineData(new[] { "--group", "Workers", "--group-id", "03910bbe-2843-47b6-bc80-ae1176bbba38" }, "correct horse battery", "jane@example.com")]
        [InlineData(new[] { "--group-id", "7133dd68-94e5-403f-89f1-50429c66fb70" }, "correct horse battery", "jane@example.com")]
        [InlineData(new[] { "--group", "Workers" }, "too short", "jane@example.com")]
        [InlineData(new[] { "--group", "Workers" }, "", "jane@example.com")]
        [InlineData(new[] { "--group", "Workers" }, "\n", "jane@example.com")]
        [InlineData(new[] { "--group", "Workers" }, "correct horse battery", "john@example.com")]
        [InlineData(new[] { "--group", "Workers" }, "correct horse battery", "John@Example.com")]
        public void UserCreatesNoUserForAnInvalidGroupOrPasswordOrAnExistingUserName(string[] groupOptions, string password, string name)
        {
            var program = new Program();
            Assert.Equal(ExitCode.Success, new Populate { Parent = program }.OnExecute(null));
            Assert.Equal(ExitCode.Success, RunUser(program, "correct horse battery", "-n", "john@example.com", "-g", "Workers", "--password-stdin"));

            int userCount;
            using (var transaction = program.Database.CreateTransaction())
            {
                userCount = new Users(transaction).Extent().Count;
            }

            Assert.Equal(ExitCode.Error, RunUser(program, password, new[] { "-n", name, "--password-stdin" }.Concat(groupOptions).ToArray()));

            using (var transaction = program.Database.CreateTransaction())
            {
                Assert.Equal(userCount, new Users(transaction).Extent().Count);
                Assert.Single(new Users(transaction).Extent().ToArray(), v => v.UserName == "john@example.com");
            }
        }

        [Theory]
        [InlineData("-p", null)]
        [InlineData("--password", null)]
        [InlineData("-p", "=")]
        [InlineData("--password", "=")]
        [InlineData("-p", ":")]
        [InlineData("--password", ":")]
        [InlineData("--password-stdin", "=")]
        [InlineData("--password-stdin", ":")]
        public void UserRejectsAPasswordInTheCommandLine(string option, string separator)
        {
            const string password = "example-password-value";
            var arguments = separator == null ? new[] { "user", option, password } : new[] { "user", option + separator + password };
            var originalOutput = Console.Out;
            var originalError = Console.Error;
            using var output = new StringWriter();
            try
            {
                Console.SetOut(output);
                Console.SetError(output);
                Assert.Equal(ExitCode.Error, Program.Main(arguments));
            }
            finally
            {
                Console.SetOut(originalOutput);
                Console.SetError(originalError);
            }

            var message = output.ToString();
            Assert.DoesNotContain(password, message);
            Assert.Contains("user --help", message);
            Assert.Contains("--password-stdin", message);
        }

        private static int RunUser(Program program, string input, params string[] arguments)
        {
            using var app = new CommandLineApplication<Program> { ModelFactory = () => program };
            app.Conventions.UseDefaultConventions();
            var original = Console.In;
            using var reader = new StringReader(input);
            try
            {
                Console.SetIn(reader);
                return app.Execute(arguments.Prepend("user").ToArray());
            }
            finally
            {
                Console.SetIn(original);
            }
        }
    }
}
