// <copyright file="IdentityOptionsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Server;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using Xunit;

    public class IdentityOptionsTests
    {
        [Fact]
        public void LockoutFollowsBestPracticeDefaults()
        {
            var identityOptions = Resolve();

            Assert.True(identityOptions.Lockout.AllowedForNewUsers);
            Assert.Equal(10, identityOptions.Lockout.MaxFailedAccessAttempts);
            Assert.Equal(TimeSpan.FromMinutes(15), identityOptions.Lockout.DefaultLockoutTimeSpan);
        }

        [Fact]
        public void PasswordPolicyFavorsLengthOverComposition()
        {
            var identityOptions = Resolve();

            Assert.Equal(12, identityOptions.Password.RequiredLength);
            Assert.False(identityOptions.Password.RequireDigit);
            Assert.False(identityOptions.Password.RequireUppercase);
            Assert.False(identityOptions.Password.RequireLowercase);
            Assert.False(identityOptions.Password.RequireNonAlphanumeric);
            Assert.Equal(4, identityOptions.Password.RequiredUniqueChars);
        }

        [Fact]
        public void ConfigurationOverridesBind()
        {
            var identityOptions = Resolve(new Dictionary<string, string>
            {
                ["Identity:Lockout:MaxFailedAccessAttempts"] = "3",
                ["Identity:Password:RequiredLength"] = "20",
            });

            Assert.Equal(3, identityOptions.Lockout.MaxFailedAccessAttempts);
            Assert.Equal(20, identityOptions.Password.RequiredLength);
        }

        private static IdentityOptions Resolve(IDictionary<string, string> configurationValues = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string>())
                .Build();

            var services = new ServiceCollection();
            services.AddAllorsServer(configuration, new StubWebHostEnvironment(), new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
                CorsOrigins = new[] { "http://localhost" },
            });

            using var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        }

        private sealed class StubWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string ApplicationName { get; set; } = "Allors.Tests";

            public IFileProvider ContentRootFileProvider { get; set; }

            public string ContentRootPath { get; set; } = System.IO.Path.GetTempPath();

            public string EnvironmentName { get; set; } = "Development";
        }
    }
}
