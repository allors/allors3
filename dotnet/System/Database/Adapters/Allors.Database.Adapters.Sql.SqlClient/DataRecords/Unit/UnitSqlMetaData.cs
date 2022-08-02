// <copyright file="UnitRoleDataRecords.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data;
    using Meta;
    using Microsoft.Data.SqlClient.Server;

    internal class UnitSqlMetaData
    {
        internal static SqlMetaData Get(string name, IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    if (roleType.Size == -1 || roleType.Size > 4000)
                    {
                        return new SqlMetaData(name, SqlDbType.NVarChar, -1);
                    }

                    return new SqlMetaData(name, SqlDbType.NVarChar, roleType.Size.Value);

                case UnitTags.Integer:
                    return new SqlMetaData(name, SqlDbType.Int);

                case UnitTags.Decimal:
                    return new SqlMetaData(name, SqlDbType.Decimal, (byte)roleType.Precision.Value, (byte)roleType.Scale.Value);

                case UnitTags.Float:
                    return new SqlMetaData(name, SqlDbType.Float);

                case UnitTags.Boolean:
                    return new SqlMetaData(name, SqlDbType.Bit);

                case UnitTags.DateTime:
                    return new SqlMetaData(name, SqlDbType.DateTime2);

                case UnitTags.Unique:
                    return new SqlMetaData(name, SqlDbType.UniqueIdentifier);

                case UnitTags.Binary:
                    if (roleType.Size == -1 || roleType.Size > 8000)
                    {
                        return new SqlMetaData(name, SqlDbType.VarBinary, -1);
                    }

                    return new SqlMetaData(name, SqlDbType.VarBinary, (long)roleType.Size);

                default:
                    throw new Exception("!UNKNOWN VALUE TYPE!");
            }
        }

    }
}
