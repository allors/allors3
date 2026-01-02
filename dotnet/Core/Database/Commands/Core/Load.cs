// <copyright file="Import.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Import the population from file")]
    public class Load
    {
        public Program Parent { get; set; }

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var client = this.Parent.ApiClient)
            {
                using (var fileStream = File.OpenRead(fileInfo.FullName))
                {
                    await client.LoadAsync(fileStream, fileInfo.Name);
                }
            }

            return ExitCode.Success;
        }
    }
}
