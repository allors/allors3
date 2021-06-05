// <copyright file="Command.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    public class XDataReader : DataReader
    {
        private readonly SqlDataReader reader;

        public XDataReader(SqlDataReader reader) => this.reader = reader;

        public override bool Read() => this.reader.Read();

        public override object this[int i] => this.reader[i];

        public override object GetValue(int i) => this.reader.GetValue(i);

        public override bool GetBoolean(int i) => this.reader.GetBoolean(i);

        public override DateTime GetDateTime(int i) => this.reader.GetDateTime(i);

        public override decimal GetDecimal(int i) => this.reader.GetDecimal(i);

        public override double GetDouble(int i) => this.reader.GetDouble(i);

        public override int GetInt32(int i) => this.reader.GetInt32(i);

        public override long GetInt64(int i) => this.reader.GetInt64(i);

        public override string GetString(int i) => this.reader.GetString(i);

        public override Guid GetGuid(int i) => this.reader.GetGuid(i);

        public override bool IsDBNull(int i) => this.reader.IsDBNull(i);

        public override void Dispose() => this.reader.Dispose();
    }
}
