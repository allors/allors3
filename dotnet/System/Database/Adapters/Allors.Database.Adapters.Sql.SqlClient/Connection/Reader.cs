// <copyright file="Command.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    public class Reader : IReader
    {
        private readonly Command command;
        private readonly SqlDataReader reader;

        public Reader(SqlDataReader reader) => this.reader = reader;

        internal Reader(Command command, SqlDataReader reader) : this(reader) => this.command = command;

        public bool Read()
        {
            try
            {
                return this.reader.Read();
            }
            catch (Exception e)
            {
                this.command?.OnStatementFailed(e);
                throw;
            }
        }

        public object this[int i] => this.ReadColumn(i, static (reader, ordinal) => reader[ordinal]);

        public object GetValue(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetValue(ordinal));

        public bool GetBoolean(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetBoolean(ordinal));

        public DateTime GetDateTime(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetDateTime(ordinal));

        public decimal GetDecimal(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetDecimal(ordinal));

        public double GetDouble(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetDouble(ordinal));

        public int GetInt32(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetInt32(ordinal));

        public long GetInt64(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetInt64(ordinal));

        public string GetString(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetString(ordinal));

        public Guid GetGuid(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.GetGuid(ordinal));

        public bool IsDBNull(int i) => this.ReadColumn(i, static (reader, ordinal) => reader.IsDBNull(ordinal));

        public void Dispose() => this.reader.Dispose();

        private T ReadColumn<T>(int ordinal, Func<SqlDataReader, int, T> read)
        {
            try
            {
                return read(this.reader, ordinal);
            }
            catch (Exception e)
            {
                this.command?.OnStatementFailed(e);
                throw;
            }
        }
    }
}
