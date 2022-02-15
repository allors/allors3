// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
        }

        public Fixture()
        {
            var database = typeof(T).Name;
            var connectionString = "Server=localhost; User Id=allors; Database=postgres; Pooling=false; CommandTimeout=300";

            int version;

            {
                // version 13+
                using var connection = new NpgsqlConnection(connectionString);
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
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                var withForce = version >= 13 ? "WITH (FORCE)" : string.Empty;
                command.CommandText = $"DROP DATABASE IF EXISTS {database} {withForce}";
                command.ExecuteNonQuery();
                connection.Close();
            }

            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE {database}";
                command.ExecuteNonQuery();
            }
        }
    }
}
