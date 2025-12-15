// <copyright file="CacheTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Xunit;

    public static class Config 
    {
        public static readonly string ConnectionString = "Server=localhost; Database=postgres; Pooling=false; CommandTimeout=300";
    }
}
