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
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/allors/Authentication/Token", "127.0.0.1"), new AuthenticationRateLimitSettings());

            Assert.Equal(AuthenticationRateLimitPolicy.LoopbackPartitionKey, partition.PartitionKey);
        }

        [Fact]
        public void RemoteClientGetsAPerIpPartition()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/allors/Authentication/Token", "203.0.113.7"), new AuthenticationRateLimitSettings());

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void PathMatchingIgnoresCase()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/allors/testauthentication/token", "203.0.113.7"), new AuthenticationRateLimitSettings());

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void IdentityLoginPathIsLimitedByDefault()
        {
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/Identity/Account/Login", "203.0.113.7"), new AuthenticationRateLimitSettings());

            Assert.Equal("authentication:203.0.113.7", partition.PartitionKey);
        }

        [Fact]
        public void PerIpLimiterExhaustsAtThePermitLimit()
        {
            var settings = new AuthenticationRateLimitSettings { PermitLimit = 2 };
            var partition = AuthenticationRateLimitPolicy.Partition(Context("/allors/Authentication/Token", "203.0.113.7"), settings);

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
            }).Build();

            var settings = AuthenticationRateLimitSettings.From(configuration);

            Assert.Equal(3, settings.PermitLimit);
            Assert.Equal(30, settings.WindowSeconds);
            Assert.Equal(42, settings.LoopbackPermitLimit);
            Assert.Contains("/allors/Authentication/Token", settings.Paths);
        }

        private static HttpContext Context(string path, string remoteIp)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            return context;
        }
    }
}
