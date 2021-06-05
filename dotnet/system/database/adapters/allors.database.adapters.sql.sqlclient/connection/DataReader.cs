// <copyright file="Command.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;

    public abstract class DataReader : IDisposable
    {
        public abstract bool Read();

        public abstract object this[int i] { get; }

        public abstract object GetValue(int i);

        public abstract bool GetBoolean(int i);

        public abstract DateTime GetDateTime(int i);

        public abstract decimal GetDecimal(int i);

        public abstract double GetDouble(int i);

        public abstract int GetInt32(int i);

        public abstract long GetInt64(int i);

        public abstract string GetString(int i);

        public abstract Guid GetGuid(int i);

        public abstract bool IsDBNull(int i);

        public abstract void Dispose();
    }
}
