// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;
    using System.IO;

    using Development.Repository.Tasks;
    using Database.Meta;

    class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        static int Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    return Default();
                case 2:
                    return Generate.Execute(MetaBuilder.Build(), args[0], args[1]).ErrorOccured ? 1 : 0;
                default:
                    return 1;
            }
        }

        private static int Default()
        {
            var metaPopulation = MetaBuilder.Build();

            string[,] config =
                {
                    { "Templates/adapters.cs.stg", "Domain/generated" },
                };

            for (var i = 0; i < config.GetLength(0); i++)
            {
                var template = config[i, 0];
                var output = config[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(metaPopulation, template, output);
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
