// <copyright file="Populate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Creates a new test population")]
    public class Populate
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            using (var client = this.Parent.ApiClient)
            {
                await client.PopulateAsync();
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}
