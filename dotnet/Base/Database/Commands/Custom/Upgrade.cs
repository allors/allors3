// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Add file contents to the index")]
    public class Upgrade
    {
        public Program Parent { get; set; }

        [Option("-f", Description = "File to load")]
        public string FileName { get; set; } = "population.xml";

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var fileInfo = new FileInfo(this.FileName);

            using (var client = this.Parent.ApiClient)
            {
                using (var fileStream = File.OpenRead(fileInfo.FullName))
                {
                    var response = await client.UpgradeAsync(fileStream, fileInfo.Name);

                    if (!response.Success)
                    {
                        Console.Error.WriteLine(response.ErrorMessage);

                        if (response.NotLoadedObjectTypeIds?.Length > 0)
                        {
                            var notLoaded = response.NotLoadedObjectTypeIds
                                .Aggregate("Could not load following ObjectTypeIds: ", (current, objectTypeId) => current + "- " + objectTypeId);
                            Console.Error.WriteLine(notLoaded);
                        }

                        if (response.NotLoadedRelationTypeIds?.Length > 0)
                        {
                            var notLoaded = response.NotLoadedRelationTypeIds
                                .Aggregate("Could not load following RelationTypeIds: ", (current, relationTypeId) => current + "- " + relationTypeId);
                            Console.Error.WriteLine(notLoaded);
                        }

                        return 1;
                    }
                }
            }

            return ExitCode.Success;
        }
    }
}
