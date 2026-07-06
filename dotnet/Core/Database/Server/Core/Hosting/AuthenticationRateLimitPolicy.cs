// <copyright file="AuthenticationRateLimitPolicy.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.RateLimiting;
    using Microsoft.AspNetCore.Http;

    public static class AuthenticationRateLimitPolicy
    {
        public const string LoopbackPartitionKey = "authentication:loopback";

        public static RateLimitPartition<string> Partition(HttpContext context, AuthenticationRateLimitSettings settings)
        {
            var path = context.Request.Path;
            if (!settings.Paths.Any(v => path.StartsWithSegments(v, StringComparison.OrdinalIgnoreCase)))
            {
                return RateLimitPartition.GetNoLimiter(string.Empty);
            }

            // The remote address is the forwarded client IP once UseForwardedHeaders has run.
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress is null || IPAddress.IsLoopback(remoteIpAddress))
            {
                // Local test rigs (e2e login loops, remote test suites) share one generous partition.
                return RateLimitPartition.GetFixedWindowLimiter(LoopbackPartitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.LoopbackPermitLimit,
                    Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                    QueueLimit = 0,
                });
            }

            return RateLimitPartition.GetFixedWindowLimiter($"authentication:{remoteIpAddress}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueLimit = 0,
            });
        }
    }
}
