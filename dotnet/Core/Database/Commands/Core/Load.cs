// <copyright file="Import.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Import the population from file")]
    public class Load
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var client = this.Parent.ApiClient)
            {
                this.Logger.Info("Loading {file}", fileInfo.FullName);
                using (var fileStream = File.OpenRead(fileInfo.FullName))
                {
                    await client.LoadAsync(fileStream, fileInfo.Name);
                }
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
