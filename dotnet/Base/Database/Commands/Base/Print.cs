// <copyright file="Print.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Print Documents")]
    public class Print
    {
        public Program Parent { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            var exitCode = ExitCode.Success;

            using var transaction = this.Parent.Database.CreateTransaction();

            var scheduler = new AutomatedAgents(transaction).System;
            transaction.Services.Get<IUserService>().User = scheduler;

            var m = this.Parent.M;
            var printDocuments = new PrintDocuments(transaction).Extent();
            printDocuments.Filter.AddNot().AddExists(m.PrintDocument.Media);

            foreach (PrintDocument printDocument in printDocuments)
            {
                var printable = printDocument.PrintableWherePrintDocument;
                if (printable == null)
                {
                    Console.Error.WriteLine($"PrintDocument with id {printDocument.Id} has no Printable object");
                    continue;
                }

                try
                {
                    printable.Print();
                    transaction.Derive();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    exitCode = ExitCode.Error;
                    Console.Error.WriteLine($"Could not print {printable}: {e}");
                }
            }

            return exitCode;
        }
    }
}
