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
        [Theory]
        [InlineData("")]
        [InlineData("0123456789012345678901234567890")]
        public void TheServerDoesNotStartWithAMissingOrShortSigningKey(string key)
        {
            using var factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = key,
            });

            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("'JwtToken:Key'", exception.Message);
        }
    }
}
