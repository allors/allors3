// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using global::Npgsql;

    public class Fixture<T>
    {
        static Fixture()
        {
            // TODO: replace timestamp with timestamp with time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO: https://www.npgsql.org/doc/release-notes/7.0.html#a-namecommandtypestoredprocedure-commandtypestoredprocedure-now-invokes-procedures-instead-of-functions
            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
        }

        public Fixture()
        {
            var database = typeof(T).Name;
            int version;

            {
                // version 13+
                using var connection = new NpgsqlConnection(Config.ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SHOW server_version";
                var scalar = command.ExecuteScalar();
                var full = scalar.ToString();
                var major = full.Substring(0, full.IndexOf("."));
                version = int.Parse(major);
                connection.Close();
            }


            {
                // version 13+
                using var connection = new NpgsqlConnection(Config.ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                var withForce = version >= 13 ? "WITH (FORCE)" : string.Empty;
                command.CommandText = $"DROP DATABASE IF EXISTS {database} {withForce}";
                command.ExecuteNonQuery();
                connection.Close();
            }

            {
                using var connection = new NpgsqlConnection(Config.ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE {database}";
                command.ExecuteNonQuery();
            }
        }
    }
}
