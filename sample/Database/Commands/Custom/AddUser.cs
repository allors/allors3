// <copyright file="AddUser.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
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

        [Option("--password-stdin", Description = "Read one line from standard input as the password")]
        public bool PasswordStdin { get; set; }

        [Option("-g|--group", Description = "Select the user group by name, such as Administrators or Workers")]
        public string Group { get; set; }

        [Option("--group-id", Description = "Select the user group by its UniqueId")]
        public Guid? GroupId { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            if ((this.Group == null) == (this.GroupId == null))
            {
                this.Logger.Error("Specify exactly one of --group <name> and --group-id <UniqueId>.");
                return ExitCode.Error;
            }

            if (!this.PasswordStdin && Console.IsInputRedirected)
            {
                this.Logger.Error("Use --password-stdin to read the password from redirected standard input.");
                return ExitCode.Error;
            }

            var password = this.PasswordStdin
                ? Console.ReadLine()
                : Prompt.GetPassword("Password (at least 12 characters, at least 4 of them different):");
            if (string.IsNullOrEmpty(password))
            {
                this.Logger.Error("Enter a password or supply it on one line with --password-stdin.");
                return ExitCode.Error;
            }

            var database = this.Parent.Database;
            var m = database.Services.Get<MetaPopulation>();

            using var transaction = database.CreateTransaction();

            var userGroups = new UserGroups(transaction);
            var matches = userGroups.Extent();
            if (this.GroupId.HasValue)
            {
                matches.Filter.AddEquals(m.UserGroup.UniqueId, this.GroupId.Value);
            }
            else
            {
                matches.Filter.AddEquals(m.UserGroup.Name, this.Group);
            }

            var selection = this.Group ?? this.GroupId.ToString();
            var count = matches.Count;
            if (count == 0)
            {
                var names = string.Join(", ", userGroups.Extent().Select(v => v.Name).OrderBy(v => v));
                this.Logger.Error("There is no user group {group}. The user groups are {names}.", selection, names);
                return ExitCode.Error;
            }

            if (count > 1)
            {
                var groups = string.Join(", ", matches.Select(v => $"{v.Name} ({v.UniqueId})").OrderBy(v => v));
                this.Logger.Error("More than one user group matches {group}. The matching groups are {groups}. Use --group or --group-id to select exactly one.", selection, groups);
                return ExitCode.Error;
            }

            var userGroup = matches[0];

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
                .WithInUserPassword(password)
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
