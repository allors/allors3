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

    [Command(Description = "Replaces every Media's EmbeddedMediaContent with an ExternalMediaContent and switches new media to external storage.")]
    public class ExternalizeMedia
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var scheduler = new AutomatedAgents(transaction).System;
            transaction.Services.Get<IUserService>().User = scheduler;

            // New media should be stored externally from now on.
            transaction.GetSingleton().StoreMediaContentExternal = true;

            var converted = Medias.ConvertEmbeddedMediaContentToExternal(transaction);

            transaction.Derive();
            transaction.Commit();

            this.Logger.Info($"Converted {converted} media to external content; new media will use external storage.");

            return ExitCode.Success;
        }
    }
}
