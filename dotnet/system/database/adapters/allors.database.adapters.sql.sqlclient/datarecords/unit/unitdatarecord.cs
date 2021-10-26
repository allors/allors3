// <copyright file="ObjectDataRecord.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient.Server;

    internal class UnitDataRecord : IEnumerable<SqlDataRecord>
    {
        private readonly Mapping mapping;
        private readonly IEnumerable<int> list;

        internal UnitDataRecord(Mapping mapping, IEnumerable<int> list)
        {
            this.mapping = mapping;
            this.list = list;
        }

        public IEnumerator<SqlDataRecord> GetEnumerator()
        {
            var objectArrayElement = this.mapping.TableTypeColumnNameForObject;
            var metaData = new SqlMetaData(objectArrayElement, SqlDbType.Int);
            var sqlDataRecord = new SqlDataRecord(metaData);

            foreach (var value in this.list)
            {
                sqlDataRecord.SetInt32(0, value);
                yield return sqlDataRecord;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
