// <copyright file="ModuleInitializer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ModuleInitializer
    {
        /// <summary>
        /// Runs once, before any type of the Npgsql adapter assembly is used, so every consumer
        /// (server, command-line tool and tests) gets the same Npgsql behaviour.
        /// </summary>
        [ModuleInitializer]
        internal static void Initialize()
        {
            // TODO: replace timestamp with timestamp with time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO: https://www.npgsql.org/doc/release-notes/7.0.html#a-namecommandtypestoredprocedure-commandtypestoredprocedure-now-invokes-procedures-instead-of-functions
            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
        }
    }
}
