// <copyright file="XParameter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;

    public class XParameterCollection : ISqlParameterCollection
    {
        private readonly SqlParameterCollection sqlCommandParameters;
        private readonly Dictionary<string, XParameter> parametersByLowercaseName;

        public XParameterCollection(SqlParameterCollection sqlCommandParameters)
        {
            this.sqlCommandParameters = sqlCommandParameters;
            this.parametersByLowercaseName = new Dictionary<string, XParameter>();
        }

        public void Add(SqlParameter sqlParameter)
        {
            this.sqlCommandParameters.Add(sqlParameter);
            this.parametersByLowercaseName.Add(sqlParameter.ParameterName, new XParameter(sqlParameter));
        }

        public ISqlParameter this[string name]
        {
            get
            {
                this.parametersByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var parameter);
                return parameter;
            }
        }
    }
}
