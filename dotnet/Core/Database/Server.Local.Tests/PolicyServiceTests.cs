// <copyright file="PolicyServiceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Data.Common;
    using System.Threading;
    using Allors.Database;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Services;
    using Xunit;

    public class PolicyServiceTests
    {
        // Writes are not idempotent: retrying a DbException after an (ambiguous) commit would
        // re-apply the write. Invoke/Push must run their delegate exactly once.

        [Fact]
        public void InvokePolicyDoesNotRetryOnDbException()
        {
            var policyService = new PolicyService();
            var count = 0;

            Assert.Throws<TestDbException>(() =>
                policyService.InvokePolicy.Execute(() =>
                {
                    count++;
                    throw new TestDbException();
                }));

            Assert.Equal(1, count);
        }

        [Fact]
        public void PushPolicyDoesNotRetryOnDbException()
        {
            var policyService = new PolicyService();
            var count = 0;

            Assert.Throws<TestDbException>(() =>
                policyService.PushPolicy.Execute(() =>
                {
                    count++;
                    throw new TestDbException();
                }));

            Assert.Equal(1, count);
        }

        [Fact]
        public void PullControllerRetriesOnDbException()
        {
            var transactionService = new FailingTransactionService();
            var controller = new PullController(transactionService, null, new PolicyService());

            Assert.Throws<InvalidOperationException>(() => controller.Post(new PullRequest(), CancellationToken.None));

            Assert.Equal(2, transactionService.Attempts);
        }

        private sealed class FailingTransactionService : ITransactionService
        {
            public int Attempts { get; private set; }

            public ITransaction Transaction
            {
                get
                {
                    if (++this.Attempts == 1)
                    {
                        throw new TestDbException();
                    }

                    // Stop after the retry without requiring a database transaction.
                    throw new InvalidOperationException();
                }
            }
        }

        private sealed class TestDbException : DbException
        {
        }
    }
}
