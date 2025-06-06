// <copyright file="Command.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using global::Npgsql;

    public class Reader : IReader
    {
        private readonly NpgsqlDataReader reader;

        public Reader(NpgsqlDataReader reader) => this.reader = reader;

        public bool Read() => this.reader.Read();

        public object this[int i] => this.reader[i];

        public object GetValue(int i) => this.reader.GetValue(i);

        public bool GetBoolean(int i) => this.reader.GetBoolean(i);

        public DateTime GetDateTime(int i) => this.reader.GetDateTime(i);

        public decimal GetDecimal(int i) => this.reader.GetDecimal(i);

        public double GetDouble(int i) => this.reader.GetDouble(i);

        public int GetInt32(int i) => this.reader.GetInt32(i);

        public long GetInt64(int i) => this.reader.GetInt64(i);

        public string GetString(int i) => this.reader.GetString(i);

        public Guid GetGuid(int i) => this.reader.GetGuid(i);

        public bool IsDBNull(int i) => this.reader.IsDBNull(i);

        public void Dispose() => this.reader.Dispose();
    }
}
