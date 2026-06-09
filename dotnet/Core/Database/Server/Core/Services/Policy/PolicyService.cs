// <copyright file="PolicyService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Data.Common;
    using Polly;

    public class PolicyService : IPolicyService
    {
        public PolicyService()
        {
            // Reads are idempotent — safe to auto-retry a transient DbException.
            var retryPolicy = Policy
                .Handle<DbException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            this.PullPolicy = retryPolicy;
            this.SyncPolicy = retryPolicy;

            // Writes are not idempotent — auto-retrying a DbException after an (ambiguous) commit
            // would re-apply the unit (double execution), so Invoke and Push are not retried.
            var noRetryPolicy = Policy.NoOp();

            this.PushPolicy = noRetryPolicy;
            this.InvokePolicy = noRetryPolicy;
        }

        public Policy PullPolicy { get; }

        public Policy SyncPolicy { get; }

        public Policy PushPolicy { get; }

        public Policy InvokePolicy { get; }
    }
}
