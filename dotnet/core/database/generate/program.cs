// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System;
    using System.IO;
    using Development.Repository.Tasks;
    using Meta;

    internal class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private static int Main()
        {
            string[,] database =
                {
                    { "Database/Templates/domain.cs.stg", "Database/Domain/generated" },
                    { "Database/Templates/uml.cs.stg", "Database/Domain.Diagrams/generated" },
                };

            string[,] workspace =
            {
                //{ "Workspace/Csharp/Templates/uml.cs.stg", "Workspace/CSharp/Diagrams/generated" },
                { "Workspace/Csharp/Templates/meta.cs.stg", "Workspace/CSharp/Meta/generated" },
                { "Workspace/Csharp/Templates/meta.lazy.cs.stg", "Workspace/CSharp/Meta.Lazy/generated" },
                { "Workspace/Csharp/Templates/domain.cs.stg", "Workspace/CSharp/Domain/generated" },

                { "../../typescript/templates/workspace.meta.ts.stg", "../../typescript/libs/workspace/meta/core/src/lib/generated" },
                { "../../typescript/templates/workspace.meta.json.ts.stg", "../../typescript/libs/workspace/meta/json/core/src/lib/generated" },
                { "../../typescript/templates/workspace.domain.ts.stg", "../../typescript/libs/workspace/domain/core/src/lib/generated" },
            };

            var metaPopulation = MetaBuilder.Build();

            for (var i = 0; i < database.GetLength(0); i++)
            {
                var template = database[i, 0];
                var output = database[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(metaPopulation, template, output);
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            var workspaceName = "Default";

            for (var i = 0; i < workspace.GetLength(0); i++)
            {
                var template = workspace[i, 0];
                var output = workspace[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(metaPopulation, template, output, workspaceName);
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            return 0;
        }

        private static void RemoveDirectory(string output)
        {
            var directoryInfo = new DirectoryInfo(output);
            if (directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch
                {
                }
            }
        }
    }
}
