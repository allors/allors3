// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System;
    using System.IO;

    using Allors.Development.Repository.Tasks;
    using Meta;

    class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        static int Main()
        {
            string[,] database =
                {
                    { "../Core/Database/Templates/meta.cs.stg", "Database/Domain/generated/meta" },
                    { "../Core/Database/Templates/domain.cs.stg", "DataBase/Domain/generated/domain" },
                    { "../Core/Database/Templates/uml.cs.stg", "DataBase/Diagrams/generated" },
                    // { "../Core/Database/Templates/uml.java.stg", "DataBase/Diagrams.java/allors" },
                };

            string[,] workspace =
            {
                //{ "../Core/Workspace/CSharp/Templates/meta.cs.stg", "Workspace/CSharp/Meta/generated" },
                //{ "../Core/Workspace/CSharp/Templates/domain.cs.stg", "Workspace/CSharp/Domain/generated" },

                { "../Core/Workspace/Typescript/Templates/meta.ts.stg", "Workspace/Typescript/libs/meta/generated/src" },
                { "../Core/Workspace/Typescript/Templates/domain.ts.stg", "Workspace/Typescript/libs/domain/generated/src" },
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
