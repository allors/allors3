// <copyright file="XParameter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Microsoft.Data.SqlClient;

    public class Parameter : ISqlParameter
    {
        public SqlParameter SqlParameter { get; }

        public Parameter(SqlParameter sqlParameter) => this.SqlParameter = sqlParameter;

        public object Value
        {
            get => this.SqlParameter.Value;

            set => this.SqlParameter.Value = value;
        }
    }
}
