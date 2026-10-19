// <copyright file="Fixture.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    /// <summary>
    /// The xUnit class fixture the workspace test classes share. It carries no state: each
    /// profile owns its connection, and the test server resets the population before every test.
    /// </summary>
    public class Fixture
    {
    }
}
