// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Microsoft.Data.SqlClient;

    public class Fixture<T>
    {
        public Fixture()
        {
            var database = typeof(T).Name;

            using var connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {database}";
            command.ExecuteNonQuery();
            command.CommandText = $"CREATE DATABASE {database}";
            command.ExecuteNonQuery();
        }
    }
}
