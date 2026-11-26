// <copyright file="AssemblyInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Xunit;

// On SQL Server or PostgreSQL every test initializes the one test database, so the test classes cannot run side
// by side; on the Memory and Snapshot adapters the suite is fast either way.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
