// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var session = this.Parent.Database.CreateTransaction();
            this.Logger.Info("Begin");

            var scheduler = new AutomatedAgents(session).System;
            session.Services.Get<IUserService>().User = scheduler;

            // Custom code
            session.Derive();
            session.Commit();

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}
