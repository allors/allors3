// <copyright file="Population.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Workspace.Tests
{
    using Allors.Database;
    using Allors.Database.Domain;

    // The objects of every test, after the sample's setup: an administrator, a worker and a work item that is
    // assigned to the worker.
    public static class Population
    {
        public const string Password = "correct horse battery";

        public static void Apply(IDatabase database)
        {
            using var transaction = database.CreateTransaction();

            var administrator = new PersonBuilder(transaction).WithUserName("admin@example.com").WithInUserPassword(Password).Build();
            new UserGroups(transaction).Administrators.AddMember(administrator);

            var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithLastName("Doe").WithUserName("jane@example.com").WithInUserPassword(Password).Build();
            new UserGroups(transaction).Workers.AddMember(jane);

            new WorkItemBuilder(transaction).WithTitle("Write the rule").WithAssignee(jane).Build();

            transaction.Derive();
            transaction.Commit();
        }
    }
}
