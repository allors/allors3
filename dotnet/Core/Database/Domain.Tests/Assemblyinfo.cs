// <copyright file="AssemblyInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Xunit;

// On SQL Server or PostgreSQL every test initialises the one database of the configuration root, so the
// test collections cannot run side by side; on the Memory adapter the suite is fast either way.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
