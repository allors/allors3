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
            var connectionString = "Server=localhost; User Id=allors; Database=postgres; Pooling=false; CommandTimeout=300";

            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"DROP DATABASE IF EXISTS {database} WITH (FORCE)";
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
