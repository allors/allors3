// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Npgsql
{
    using global::Npgsql;

    public class Fixture<T>
    {
        public Fixture()
        {
            var database = typeof(T).Name;

            using var connection = new NpgsqlConnection($"Server=localhost; User Id=test; Password=test; Database=postgres; Pooling=false; CommandTimeout=300");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {database}";
            command.ExecuteNonQuery();
            command.CommandText = $"CREATE DATABASE {database}";
            command.ExecuteNonQuery();
        }
    }
}
