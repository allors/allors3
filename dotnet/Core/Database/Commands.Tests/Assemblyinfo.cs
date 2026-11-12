// <copyright file="AssemblyInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Xunit;

// The tests set environment variables and the working directory, which a process has once.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
