// <copyright file="StartupTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class StartupTests
    {
        [Fact]
        public void TheServerDoesNotStartWithoutTheKeyOfTheTokens()
        {
            // An empty value overrides a key that the configuration root or the environment may hold.
            using var factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = string.Empty,
            });

            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("'JwtToken:Key'", exception.Message);
        }
    }
}
