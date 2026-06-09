// <copyright file="PolicyServiceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Data.Common;
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

        private sealed class TestDbException : DbException
        {
        }
    }
}
