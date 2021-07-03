// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using global::Npgsql;

    public class Fixture<T>
    {
        public Fixture()
        {
            var database = typeof(T).Name;

            using var connection = new NpgsqlConnection($"Server=localhost; User Id=allors; Database=postgres; Pooling=false; CommandTimeout=300");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @$"
SELECT 
    pg_terminate_backend(pid) 
FROM 
    pg_stat_activity 
WHERE 
    -- don't kill my own connection!
    pid <> pg_backend_pid()
    -- don't kill the connections to other databases
    AND datname = '${database}'
    ;";
            command.ExecuteNonQuery();
            connection.Close();

            connection.Open();
            command.CommandText = $"DROP DATABASE IF EXISTS {database}";
            command.ExecuteNonQuery();
            connection.Close();

            connection.Open();
            command.CommandText = $"CREATE DATABASE {database}";
            command.ExecuteNonQuery();
        }
    }
}
