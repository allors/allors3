// <copyright file="Save.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Save the population to file")]
    public class Save
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to save")]
        public string FileName { get; set; } = "population.xml";

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var client = this.Parent.ApiClient)
            {
                this.Logger.Info("Saving to {file}", fileInfo.FullName);
                using (var stream = await client.SaveAsync())
                {
                    using (var fileStream = File.Create(fileInfo.FullName))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
