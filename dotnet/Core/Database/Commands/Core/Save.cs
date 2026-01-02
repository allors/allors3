// <copyright file="Save.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Save the population to file")]
    public class Save
    {
        public Program Parent { get; set; }

        [Option("-f", Description = "File to save")]
        public string FileName { get; set; } = "population.xml";

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var client = this.Parent.ApiClient)
            {
                using (var stream = await client.SaveAsync())
                {
                    using (var fileStream = File.Create(fileInfo.FullName))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }

            return ExitCode.Success;
        }
    }
}
