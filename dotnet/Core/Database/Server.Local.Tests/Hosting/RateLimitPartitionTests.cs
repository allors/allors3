// <copyright file="RateLimitPartitionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Collections.Generic;
    using System.Net;
    using Allors.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class RateLimitPartitionTests
    {
        [Fact]
        public void NonAuthenticationPathIsNotLimited()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/allors/pull", "203.0.113.7"), new AuthenticationRateLimitSettings());

            Assert.Equal(string.Empty, partition.PartitionKey);
        }

        [Fact]
        public void LoopbackClientGetsTheSharedHeadroomPartition()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/sign-in", "127.0.0.1"), Settings("/sign-in"));

            Assert.Equal(AuthenticationRateLimitPolicy.LoopbackPartitionKey, partition.PartitionKey);
        }

        [Fact]
        public void RemoteClientGetsAPerIpPartition()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/sign-in", "203.0.113.7"), Settings("/sign-in"));

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void PathMatchingIgnoresCase()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/SIGN-IN", "203.0.113.7"), Settings("/sign-in"));

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void IdentityLoginPathIsAnAuthenticationPath()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/Identity/Account/Login", "203.0.113.7"), Settings(IdentityPaths.Authentication));

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void PerIpLimiterExhaustsAtThePermitLimit()
        {
            var settings = new AuthenticationRateLimitSettings { PermitLimit = 2, Paths = new[] { "/sign-in" } };
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/sign-in", "203.0.113.7"), settings);

            using var limiter = partition.Factory(partition.PartitionKey);

            Assert.True(limiter.AttemptAcquire().IsAcquired);
            Assert.True(limiter.AttemptAcquire().IsAcquired);
            Assert.False(limiter.AttemptAcquire().IsAcquired);
        }

        [Fact]
        public void SettingsBindFromConfiguration()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Security:AuthenticationRateLimit:PermitLimit"] = "3",
                ["Security:AuthenticationRateLimit:WindowSeconds"] = "30",
                ["Security:AuthenticationRateLimit:LoopbackPermitLimit"] = "42",
                ["Security:AuthenticationRateLimit:Paths:0"] = "/sign-in",
            }).Build();

            var settings = AuthenticationRateLimitSettings.From(configuration);

            Assert.Equal(3, settings.PermitLimit);
            Assert.Equal(30, settings.WindowSeconds);
            Assert.Equal(42, settings.LoopbackPermitLimit);
            Assert.Equal(new[] { "/sign-in" }, settings.Paths);
        }

        private static AuthenticationRateLimitSettings Settings(params string[] paths) => new() { Paths = paths };

        private static HttpContext Context(string path, string remoteIp)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            return context;
        }
    }
}
