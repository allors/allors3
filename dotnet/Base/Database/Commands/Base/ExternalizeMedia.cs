// <copyright file="ExternalizeMedia.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Replaces every Media's EmbeddedMediaContent with an ExternalMediaContent (moving bytes from the database to external storage). Whether new media is stored externally is configured in code (see DefaultDatabaseServices).")]
    public class ExternalizeMedia
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var scheduler = new AutomatedAgents(transaction).System;
            transaction.Services.Get<IUserService>().User = scheduler;

            var converted = Medias.ConvertEmbeddedMediaContentToExternal(transaction);

            transaction.Derive();
            transaction.Commit();

            this.Logger.Info($"Converted {converted} media to external content. New media is stored externally only when the host's IMediaContentFactory is configured for it (see DefaultDatabaseServices).");

            return ExitCode.Success;
        }
    }
}
