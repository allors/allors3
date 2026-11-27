// <copyright file="AssemblyInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Xunit;

// The domain tests share one collection, so they run one at a time. DatabaseFixtureTests changes environment
// variables that DatabaseFixture reads, so no two collections may run side by side either.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
