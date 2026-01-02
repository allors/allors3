// <copyright file="Custom.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            using var session = this.Parent.Database.CreateTransaction();

            var scheduler = new AutomatedAgents(session).System;
            session.Services.Get<IUserService>().User = scheduler;

            // Custom code
            session.Derive();
            session.Commit();

            return ExitCode.Success;
        }
    }
}
