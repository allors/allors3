// <copyright file="AddUser.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.AspNetCore.Identity;
    using NLog;

    // A user that can sign in to the server: a person with a user name and a password, in a user group that
    // gives it access. The rule of the password hashes it and checks its strength.
    [Command("user", Description = "Create a user with a password in a user group")]
    public class AddUser
    {
        public Program Parent { get; set; }

        [Required]
        [Option("-n|--name", Description = "The user name")]
        public string Name { get; set; }

        [Required]
        [Option("-p|--password", Description = "The password: at least 12 characters, at least 4 of them different")]
        public string Password { get; set; }

        [Required]
        [Option("-g|--group", Description = "The user group, such as Administrators or Workers")]
        public string Group { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            var database = this.Parent.Database;
            var m = database.Services.Get<MetaPopulation>();

            using var transaction = database.CreateTransaction();

            var userGroups = new UserGroups(transaction);
            var userGroup = userGroups.FindBy(m.UserGroup.Name, this.Group);
            if (userGroup == null)
            {
                var names = string.Join(", ", userGroups.Extent().Select(v => v.Name).OrderBy(v => v));
                this.Logger.Error("There is no user group {group}. The user groups are {names}.", this.Group, names);
                return ExitCode.Error;
            }

            // The sign-in finds a user by the name that ASP.NET Core Identity normalizes, so it would not tell
            // apart two users whose names differ only in case.
            var normalizedName = new UpperInvariantLookupNormalizer().NormalizeName(this.Name);
            if (new Users(transaction).FindBy(m.User.NormalizedUserName, normalizedName) != null)
            {
                this.Logger.Error("The user {name} exists already.", this.Name);
                return ExitCode.Error;
            }

            var person = new PersonBuilder(transaction)
                .WithUserName(this.Name)
                .WithInUserPassword(this.Password)
                .Build();
            userGroup.AddMember(person);

            var validation = transaction.Derive(false);
            if (validation.HasErrors)
            {
                foreach (var error in validation.Errors)
                {
                    this.Logger.Error(error.Message);
                }

                transaction.Rollback();
                return ExitCode.Error;
            }

            transaction.Commit();

            this.Logger.Info("Created the user {name} in the user group {group}", this.Name, userGroup.Name);
            return ExitCode.Success;
        }
    }
}
