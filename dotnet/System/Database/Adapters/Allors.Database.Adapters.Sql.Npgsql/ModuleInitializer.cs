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
        /// Runs once when the Npgsql adapter assembly is loaded, before any of its types are used,
        /// so every consumer (server, commands and tests) gets the same Npgsql behavior.
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
