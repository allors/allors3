// <copyright file="ExceptionHandlerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using Allors.Server;
    using Microsoft.IdentityModel.Tokens;
    using Xunit;

    public class ExceptionHandlerTests
    {
        [Fact]
        public void ProductionMessageDoesNotLeakExceptionDetail()
        {
            var error = new Exception("secret-internal-detail");

            var message = ExceptionHandler.ErrorMessage(error, isDevelopment: false);

            Assert.DoesNotContain("secret-internal-detail", message);
        }

        [Fact]
        public void ProductionTokenExpiredMessageDoesNotLeakExceptionDetail()
        {
            var error = new SecurityTokenExpiredException("token-secret-detail");

            var message = ExceptionHandler.ErrorMessage(error, isDevelopment: false);

            Assert.DoesNotContain("token-secret-detail", message);
        }

        [Fact]
        public void DevelopmentMessageIncludesExceptionDetail()
        {
            var error = new Exception("dev-visible-detail");

            var message = ExceptionHandler.ErrorMessage(error, isDevelopment: true);

            Assert.Contains("dev-visible-detail", message);
        }
    }
}
