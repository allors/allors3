// <copyright file="ObjectDataRecord.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{

    // TODO: Use TVP for IN
    //internal class UnitListDataRecord : IEnumerable<SqlDataRecord>
    //{
    //    private readonly Mapping mapping;
    //    private readonly UnitList list;

    //    internal UnitListDataRecord(Mapping mapping, UnitList list)
    //    {
    //        this.mapping = mapping;
    //        this.list = list;
    //    }

    //    public IEnumerator<SqlDataRecord> GetEnumerator()
    //    {
    //        var objectArrayElement = this.mapping.TableTypeColumnNameForObject;
    //        var metaData = UnitSqlMetaData.Get(this.mapping.TableTypeColumnNameForRole, this.list.RoleType);
    //        var sqlDataRecord = new SqlDataRecord(metaData);

    //        foreach (var value in this.list.Values)
    //        {
    //            sqlDataRecord.SetValue(0, value ?? DBNull.Value);
    //            yield return sqlDataRecord;
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    //}
}
