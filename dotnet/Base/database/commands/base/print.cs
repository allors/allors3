// <copyright file="Print.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Print Documents")]
    public class Print
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            var exitCode = ExitCode.Success;

            using var transaction = this.Parent.Database.CreateTransaction();
            this.Logger.Info("Begin");

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
                    this.Logger.Warn($"PrintDocument with id {printDocument.Id} has no Printable object");
                    continue;
                }

                try
                {
                    printable.Print();

                    if (printable.ExistPrintDocument)
                    {
                        this.Logger.Info($"Printed {printable.PrintDocument.Media.FileName}");
                    }

                    transaction.Derive();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    exitCode = ExitCode.Error;

                    this.Logger.Error(e, $"Could not print {printable}");
                }
            }

            this.Logger.Info("End");

            return exitCode;
        }
    }
}
