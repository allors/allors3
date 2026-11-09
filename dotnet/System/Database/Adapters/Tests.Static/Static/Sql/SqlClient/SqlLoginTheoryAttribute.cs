// <copyright file="SqlLoginTheoryAttribute.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;
    using Xunit;

    /// <summary>
    /// A theory that connects with a SQL Server login. LocalDB accepts Windows authentication only, so the
    /// theory is skipped when the admin connection names a LocalDB instance, as it does by default on Windows.
    /// </summary>
    public sealed class SqlLoginTheoryAttribute : TheoryAttribute
    {
        public SqlLoginTheoryAttribute()
        {
            try
            {
                var dataSource = new SqlConnectionStringBuilder(Provisioning.ConnectionString("master")).DataSource;
                if (dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase))
                {
                    this.Skip = "LocalDB accepts Windows authentication only, and this theory connects with a SQL Server login.";
                }
            }
            catch (InvalidOperationException)
            {
                // No admin connection: the theory runs and reports the missing connection itself.
            }
        }
    }
}
