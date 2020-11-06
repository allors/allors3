// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    public class Many2OneTests : Workspace.Many2OneTests
    {
        public Many2OneTests() => this.Profile = new Profile();

        protected override IProfile Profile { get; }
    }
}
